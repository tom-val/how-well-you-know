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
    public class QuestionController : ControllerBase
    {
        private QuestionsService questionsService;
        public QuestionController(QuestionsService questionsService)
        {
            this.questionsService = questionsService;
        }

        [HttpPost]
        public Task<Guid> Create(CreateQuestionRequest request, [FromRoute] Guid gameId)
        {
            return questionsService.CreateQuestion(request, gameId);
        }
    }
}