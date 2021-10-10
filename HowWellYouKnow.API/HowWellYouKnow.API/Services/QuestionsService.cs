using HowWellYouKnow.API.Hubs;
using HowWellYouKnow.API.Requests;
using HowWellYouKnow.Domain.Dtos;
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
        private GameRepository gameRepository;
        private readonly IHubContext<QuestionsHub> hubContext;
        public QuestionsService(GameRepository gameRepository, IHubContext<QuestionsHub> hubContext)
        {
            this.hubContext = hubContext;
            this.gameRepository = gameRepository;
        }

        public async Task<Guid> CreateQuestion(CreateQuestionRequest request, Guid gameId)
        {

            var game = await gameRepository.GetGameWithQuestions(gameId);

            var question = game.AddQuestion(request.Variants, request.Name, request.MultipleAnswers);

            await gameRepository.SaveChanges();

            await hubContext.Clients.All.SendAsync(gameId.ToString(), new QuestionDto
            {
                Id = question.Id,
                Name = question.Name,
                Order = question.Order,
                MultipleAnswers = question.MultipleAnswers,
                Variants = question.Variants.Select(v => new QuestionVariantDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    Notation = v.Notation
                }).OrderBy(x => x.Notation).ToList()
            });


            return question.Id;
        }
    }
}
