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
        private QuestionRepository questionRepository;
        private GameRepository gameRepository;
        private readonly IHubContext<QuestionsHub> hubContext;
        public QuestionsService(GameRepository gameRepository, QuestionRepository questionRepository, IHubContext<QuestionsHub> hubContext)
        {
            this.questionRepository = questionRepository;
            this.hubContext = hubContext;
            this.gameRepository = gameRepository;
        }

        public async Task<Guid> CreateQuestion(CreateQuestionRequest request, Guid gameId, Guid userId)
        {

            if (request.Variants.Count < 2)
            {
                throw new ValidationException("Invalid number of varitants");
            }

            var game = await gameRepository.GetGameWithQuestions(gameId);

            var question = new Question
            {
                Name = request.Name,
                MultipleAnswers = request.MultipleAnswers,
                GameId = gameId,
                Order = game.Questions.Count + 1,
                Variants = request.Variants.Select(v => new QuestionVariant
                {
                    Name = v.Name,
                    Notation = v.Notation,
                }).ToList()
            };

            game.Questions.Add(question);
            game.LastQuestion = question;

            await questionRepository.SaveChanges();

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
