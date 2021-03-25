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

            if(!question.MultipleAnswers && request.QuestionVariants.Count != 1)
            {
                throw new ValidationException("Wrong variant count");
            }

            if(question.Answers.Any(x => x.UserId == userId))
            {
                throw new ValidationException("Answer already saved");
            }

            var questionVariants = question.Variants.Where(x => request.QuestionVariants.Contains(x.Id)).ToList();

            var answer = new Answer
            {
                QuestionId = request.QuestionId,
                UserId = userId,
                QuestionVariants = questionVariants
            };

            await answerRepository.SaveAnswer(answer);
            await answerRepository.SaveChanges();

            return answer.Id;
        }

        public async Task<Guid> SaveUserGuess(UserGuessRequest request, Guid userId, Guid gameId)
        {
            var question = await questionRepository.GetQuestion(request.QuestionId);

            if (!question.MultipleAnswers && request.QuestionVariants.Count != 1)
            {
                throw new ValidationException("Wrong variant count");
            }

            if (question.Guesses.Any(x => x.UserId == userId && x.GuessUserId == request.GuessUser))
            {
                await gameStatusService.CheckIfAllAnswered(gameId);
                throw new ValidationException("Guess already saved");
            }


            var questionVariants = question.Variants.Where(x => request.QuestionVariants.Contains(x.Id)).ToList();

            var answer = new Guess
            {
                QuestionId = request.QuestionId,
                UserId = userId,
                QuestionVariants = questionVariants,
                GuessUserId = request.GuessUser
            };

            await guessRepository.SaveGuess(answer);
            await guessRepository.SaveChanges();

            await gameStatusService.CheckIfAllAnswered(gameId);

            return answer.Id;
        }
    }
}
