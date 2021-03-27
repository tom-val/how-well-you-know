using HowWellYouKnow.API.Hubs;
using HowWellYouKnow.Domain.Models;
using HowWellYouKnow.Infrastructure.Repositories;
using HowWellYouKnow.Infrastructure.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HowWellYouKnow.API.Services
{
    public class GameStatusService
    {
        private GameReadService gameReadService;
        private GameRepository gameRepository;
        private AnswerResultRepository answerResultRepository;
        private readonly IHubContext<GameStateHub> hubContext;
        public GameStatusService(GameReadService gameReadService, IHubContext<GameStateHub> hubContext, GameRepository gameRepository, AnswerResultRepository answerResultRepository)
        {
            this.gameReadService = gameReadService;
            this.hubContext = hubContext;
            this.gameRepository = gameRepository;
            this.answerResultRepository = answerResultRepository;
            this.hubContext = hubContext;
        }

        public async Task BroadcastGameState(Guid gameId)
        {
            var gameState = await gameReadService.GetGameState(gameId);
            await hubContext.Clients.All.SendAsync(gameId.ToString(), gameState);
        }

        public async Task CheckIfAllAnswered(Guid gameId)
        {
            var game = await gameRepository.GetGameWithGameState(gameId);

            var gameUsers = game.JoinedUsers.Count();

            if (gameUsers * (gameUsers - 1) != game.GameState.CurrentQuestion.Guesses.Count)
            {
                return;
            }

            var userAnswerResults = game.JoinedUsers.Select(u => {
                var guessAnswers = game.GameState.CurrentQuestion.Guesses.Where(g => g.GuessUserId != u.Id && g.UserId == u.Id).Select(guess =>
                {
                    var answer = game.GameState.CurrentQuestion.Answers.FirstOrDefault(x => x.UserId == guess.GuessUserId);

                    var answerString = string.Join(";", answer.QuestionVariants.OrderBy(x => x.Id).Select(x => x.Name));
                    var guessString = string.Join(";", guess.QuestionVariants.OrderBy(x => x.Id).Select(x => x.Name));

                    var allMatch = guessString == answerString;

                    if (allMatch)
                    {
                        var gameScore = game.GameState.GameScores.FirstOrDefault(x => x.UserId == u.Id);
                        gameScore.CurrentScore += game.GameState.CurrentQuestion.MultipleAnswers ? 2 : 1;
                    }

                    return new UserAnswerResult
                    {
                        UserId = u.Id,
                        QuestionId = game.GameState.CurrentQuestionId.Value,
                        AnswerQuestionVariants = answer.QuestionVariants,
                        GuessQuestionVariants = guess.QuestionVariants,
                        GuessUserId = guess.GuessUserId,
                        Correct = allMatch
                    };
                });
                return guessAnswers;
            }).SelectMany(x => x).ToList();

            game.GameState.CurrentGameState = Domain.Enums.CurrentGameState.QuestionReview;

            gameRepository.UpdateGame(game);

            await answerResultRepository.SaveAnswerResult(userAnswerResults);
            await answerResultRepository.SaveChanges();

            await BroadcastGameState(gameId);
        }
    }
}
