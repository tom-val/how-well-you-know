using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HowWellYouKnow.API.Requests;
using HowWellYouKnow.API.Services;
using HowWellYouKnow.Domain.Dtos;
using HowWellYouKnow.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HowWellYouKnow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private GameService gameService;
        private GameReadService gameReadService;
        public GameController(GameService gameService, GameReadService gameReadService)
        {
            this.gameService = gameService;
            this.gameReadService = gameReadService;
        }

        [HttpPost]
        public Task<Guid> Create(CreateGameRequest request, [FromHeader] Guid userId)
        {
            return gameService.CreateNewGame(request, userId);
        }

        [HttpGet("{id}")]
        public Task<GameDto> GetGame([FromRoute] Guid id, [FromHeader] Guid userId)
        {
            return gameService.GetGame(id, userId);
        }

        [HttpGet("{id}/state")]
        public Task<GameStateDto> GetGameState([FromRoute] Guid id)
        {
            return gameReadService.GetGameState(id);
        }

        [HttpPost("{id}/start")]
        public async Task StartGame([FromRoute] Guid id)
        {
            await gameService.StartGame(id);
        }

        [HttpPost("{id}/nextQuestion")]
        public async Task NextQuestion([FromRoute] Guid id)
        {
            await gameService.NextQuestion(id);
        }
        [HttpPost("{id}/endGame")]
        public async Task EndGame([FromRoute] Guid id)
        {
            await gameService.EndGame(id);
        }

    }
}