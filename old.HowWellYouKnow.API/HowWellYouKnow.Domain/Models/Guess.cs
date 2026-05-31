using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace HowWellYouKnow.Domain.Models
{
    public class Guess : BaseEntity
    {
        private Guess() { }
        public Guid QuestionId { get; set; }
        public Question Question { get; set; }
        public List<QuestionVariant> QuestionVariants { get; set; }
        public User User { get; set; }
        public Guid UserId { get; set; }
        public User GuessUser { get; set; }
        public Guid GuessUserId { get; set; }

        public static Guess Create(Guid guessUserId, List<Guid> questionVariantIds, Guid userId, Question question)
        {
            if (!question.MultipleAnswers && questionVariantIds.Count != 1)
            {
                throw new ValidationException("Wrong variant count");
            }

            if (question.Guesses.Any(x => x.UserId == userId && x.GuessUserId == guessUserId))
            {
                throw new ValidationException("Guess already saved");
            }

            var questionVariants = question.Variants.Where(x => questionVariantIds.Contains(x.Id)).ToList();

            return new Guess
            {
                QuestionId = question.Id,
                UserId = userId,
                QuestionVariants = questionVariants,
                GuessUserId = guessUserId
            };
        }
    }
}
