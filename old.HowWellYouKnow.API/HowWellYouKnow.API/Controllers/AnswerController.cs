using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HowWellYouKnow.API.Requests;
using HowWellYouKnow.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HowWellYouKnow.API.Controllers
{
    [Route("api/game/{gameId}/[controller]")]
    [ApiController]
    public class AnswerController : ControllerBase
    {
        private AnswerService answerService;
        public AnswerController(AnswerService answerService)
        {
            this.answerService = answerService;
        }

        [HttpPost("answer")]
        public Task<Guid> Answer(UserAnswerRequest request, [FromRoute] Guid gameId, [FromHeader] Guid userId)
        {
            return answerService.SaveUserAnswer(request, userId);
        }

        [HttpPost("guess")]
        public Task<Guid> Guess(UserGuessRequest request, [FromRoute] Guid gameId, [FromHeader] Guid userId)
        {
            return answerService.SaveUserGuess(request, userId, gameId);
        }
    }
}