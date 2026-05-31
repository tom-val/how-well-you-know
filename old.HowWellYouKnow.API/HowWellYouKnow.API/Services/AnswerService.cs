using HowWellYouKnow.API.Requests;
using HowWellYouKnow.Domain.Models;
using HowWellYouKnow.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HowWellYouKnow.API.Services
{
    public class AnswerService
    {
        private AnswerRepository answerRepository;
        private GuessRepository guessRepository;
        private QuestionRepository questionRepository;
        private GameStatusService gameStatusService;

        public AnswerService(AnswerRepository answerRepository, QuestionRepository questionRepository, GuessRepository guessRepository, GameStatusService gameStatusService)
        {
            this.answerRepository = answerRepository;
            this.questionRepository = questionRepository;
            this.guessRepository = guessRepository;
            this.gameStatusService = gameStatusService;
        }

        public async Task<Guid> SaveUserAnswer(UserAnswerRequest request, Guid userId)
        {
            var question = await questionRepository.GetQuestion(request.QuestionId);

            var answer = Answer.Create(question, request.QuestionVariants, userId);

            await answerRepository.SaveAnswer(answer);
            await answerRepository.SaveChanges();

            return answer.Id;
        }

        public async Task<Guid> SaveUserGuess(UserGuessRequest request, Guid userId, Guid gameId)
        {
            var question = await questionRepository.GetQuestion(request.QuestionId);

            var guess = Guess.Create(request.GuessUser, request.QuestionVariants, userId, question);

            await guessRepository.SaveGuess(guess);
            await guessRepository.SaveChanges();

            await gameStatusService.CheckIfAllAnswered(gameId);

            return guess.Id;
        }
    }
}
