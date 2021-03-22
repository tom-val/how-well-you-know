using HowWellYouKnow.API.Hubs;
using HowWellYouKnow.API.Requests;
using HowWellYouKnow.Domain.Models;
using HowWellYouKnow.Infrastructure.Repositories;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HowWellYouKnow.API.Services
{
    public class QuestionsService
    {
        private QuestionRepository questionRepository;
        private readonly IHubContext<QuestionsHub> hubContext;
        public QuestionsService(QuestionRepository questionRepository, IHubContext<QuestionsHub> hubContext)
        {
            this.questionRepository = questionRepository;
            this.hubContext = hubContext;
        }

        public async Task<Guid> CreateQuestion(CreateQuestionRequest request, Guid gameId, Guid userId)
        {

            if (request.Variants.Count < 2)
            {
                throw new ValidationException("Invalid number of varitants");
            }

            var question = new Question {
               Name = request.Name,
               MultipleAnswers = request.MultipleAnswers,
               GameId = gameId,
               Variants = request.Variants.Select(v => new QuestionVariant
                {
                    Name = v.Name,
                    Notation = v.Notation,
                }).ToList()
            };

            await questionRepository.SaveQuestion(question);
            await questionRepository.SaveChanges();

            await hubContext.Clients.All.SendAsync(gameId.ToString(), new {
                    question.Id,
                    question.Name,
            });

            return question.Id;
        }
    }
}
