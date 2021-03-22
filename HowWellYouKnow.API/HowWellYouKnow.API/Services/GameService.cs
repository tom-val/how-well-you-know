using HowWellYouKnow.API.Requests;
using HowWellYouKnow.Domain.Dtos;
using HowWellYouKnow.Domain.Enums;
using HowWellYouKnow.Domain.Models;
using HowWellYouKnow.Infrastructure.Repositories;
using HowWellYouKnow.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HowWellYouKnow.API.Services
{
    public class GameService
    {
        private GameRepository gameRepository;
        private GameReadService gameReadService;
        private UserRepository userRepository;
        private GameStatusService gameStatusService;
        public GameService(GameRepository gameRepository, GameReadService gameReadService, UserRepository userRepository, GameStatusService gameStatusService)
        {
            this.gameRepository = gameRepository;
            this.gameReadService = gameReadService;
            this.userRepository = userRepository;
            this.gameStatusService = gameStatusService;
        }

        public async Task<Guid> CreateNewGame(CreateGameRequest request, Guid userId)
        {
            var user = await userRepository.GetUser(userId);

            var game = new Game
            {
                Name = request.Name,
                CreatedByUserId = userId,
                JoinedUsers = new List<User> { user },
                GameState = new GameState
                {
                    CurrentGameState = CurrentGameState.NotStarted,
                }
            };

            await gameRepository.SaveGame(game);
            await gameRepository.SaveChanges();

            return game.Id;
        }

        public async Task<GameDto> GetGame(Guid gameId, Guid userId)
        {
            var gameStatus = await gameReadService.GetGameStatus(gameId);

            if (gameStatus.GameState == CurrentGameState.NotStarted && gameStatus.JoinedUsers.All(x => x.Id != userId))
            {
                var user = await userRepository.GetUser(userId);
                var game = await gameRepository.GetGame(gameId);

                game.JoinedUsers.Add(user);
                await gameRepository.SaveChanges();
            }

            return await gameReadService.GetGame(gameId);
        }

        public async Task<Guid> StartGame(Guid gameId)
        {
            var game = await gameRepository.GetGame(gameId);

            if (game.GameState.CurrentGameState != CurrentGameState.NotStarted)
            {
                throw new ValidationException("You cannot start game that already started");
            }

            if (game.JoinedUsers.Count < 2)
            {
                throw new ValidationException("You need at least 2 users to start the game");
            }

            game.GameState.CurrentGameState = CurrentGameState.AnsweringQuestion;
            game.GameState.CurrentQuestion = game.Questions.OrderBy(x => x.Name).First();
            game.GameState.GameScores = game.JoinedUsers.Select(u => new UserGameScore
            {
                UserId = u.Id,
                CurrentScore = 0
            }).ToList();

            await gameRepository.SaveChanges();

            await gameStatusService.BroadcastGameState(gameId);

            return game.Id;
        }

        public async Task<Guid> NextQuestion(Guid gameId)
        {
            var game = await gameRepository.GetGame(gameId);

            if (game.GameState.CurrentGameState != CurrentGameState.QuestionReview)
            {
                throw new ValidationException("You cannot get next question");
            }


            game.GameState.CurrentGameState = CurrentGameState.AnsweringQuestion;
            game.GameState.CurrentQuestion = game.Questions.OrderBy(x => x.Name).SkipWhile(item => item.Id != game.GameState.CurrentQuestionId).Skip(1).FirstOrDefault();

            await gameRepository.SaveChanges();

            await gameStatusService.BroadcastGameState(gameId);

            return game.Id;
        }

        public async Task<Guid> EndGame(Guid gameId)
        {
            var game = await gameRepository.GetGame(gameId);

            game.GameState.CurrentGameState = CurrentGameState.GameReview;
            game.GameState.CurrentQuestionId = null;

            await gameRepository.SaveChanges();

            await gameStatusService.BroadcastGameState(gameId);

            return game.Id;
        }
    }
}
