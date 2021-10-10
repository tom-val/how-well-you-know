using HowWellYouKnow.API.Hubs;
using HowWellYouKnow.API.Requests;
using HowWellYouKnow.Domain.Dtos;
using HowWellYouKnow.Domain.Enums;
using HowWellYouKnow.Domain.Models;
using HowWellYouKnow.Infrastructure.Repositories;
using HowWellYouKnow.Infrastructure.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HowWellYouKnow.API.Services
{
    public class GameService
    {
        private GameRepository gameRepository;
        private GameReadService gameReadService;
        private UserRepository userRepository;
        private GameStatusService gameStatusService;
        private readonly IHubContext<UserHub> hubContext;
        public GameService(GameRepository gameRepository, GameReadService gameReadService, UserRepository userRepository, GameStatusService gameStatusService, IHubContext<UserHub> hubContext)
        {
            this.gameRepository = gameRepository;
            this.gameReadService = gameReadService;
            this.userRepository = userRepository;
            this.gameStatusService = gameStatusService;
            this.hubContext = hubContext;
        }

        public async Task<Guid> CreateNewGame(CreateGameRequest request, Guid userId)
        {
            var user = await userRepository.GetUser(userId);

            var game = Game.Create(request.Name, user);

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
                await hubContext.Clients.All.SendAsync(gameId.ToString(), new { user.Id, user.Name });
            }

            return await gameReadService.GetGame(gameId);
        }

        public async Task<Guid> StartGame(Guid gameId)
        {
            var game = await gameRepository.GetGame(gameId);

            game.StartGame();

            await gameRepository.SaveChanges();

            await gameStatusService.BroadcastGameState(gameId);

            return game.Id;
        }

        public async Task<Guid> NextQuestion(Guid gameId)
        {
            var game = await gameRepository.GetGame(gameId);

            game.NextQuestion();

            await gameRepository.SaveChanges();

            await gameStatusService.BroadcastGameState(gameId);

            return game.Id;
        }

        public async Task<Guid> EndGame(Guid gameId)
        {
            var game = await gameRepository.GetGame(gameId);

            game.EndGame();

            await gameRepository.SaveChanges();

            await gameStatusService.BroadcastGameState(gameId);

            return game.Id;
        }
    }
}
