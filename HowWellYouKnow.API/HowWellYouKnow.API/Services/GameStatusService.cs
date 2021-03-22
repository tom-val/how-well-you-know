﻿using HowWellYouKnow.API.Hubs;
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

            if (gameUsers != game.GameState.CurrentQuestion.Guesses.Count)
            {
                return;
            }

            var userAnswerResults = game.JoinedUsers.Select(u => {
                var guess = game.GameState.CurrentQuestion.Guesses.First(x => x.UserId == u.Id);
                var answer = game.GameState.CurrentQuestion.Answers.FirstOrDefault(x => x.UserId == guess.GuessUserId);

                var allMatch = guess.QuestionVariants.Except(answer.QuestionVariants).Count() == 0;

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
                    Correct = allMatch
                };
            }).ToList();

            game.GameState.CurrentGameState = Domain.Enums.CurrentGameState.QuestionReview;

            gameRepository.UpdateGame(game);

            await answerResultRepository.SaveAnswerResult(userAnswerResults);
            await answerResultRepository.SaveChanges();

            await BroadcastGameState(gameId);
        }
    }
}
