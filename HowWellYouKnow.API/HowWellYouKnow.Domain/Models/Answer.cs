using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace HowWellYouKnow.Domain.Models
{
    public class Answer : BaseEntity
    {
        private Answer() { }
        public Guid QuestionId { get; private set; }
        public Question Question { get; private set; }
        public List<QuestionVariant> QuestionVariants { get; private set; }
        public User User { get; private set; }
        public Guid UserId { get; private set; }

        public static Answer Create(Question question, List<Guid> questionVariantIds, Guid userId)
        {
            if (question is null)
            {
                throw new ValidationException("Question is invalid.");
            }

            if (!question.MultipleAnswers && questionVariantIds.Count != 1)
            {
                throw new ValidationException("Wrong variant count.");
            }

            if (question.Answers.Any(x => x.UserId == userId))
            {
                throw new ValidationException("Answer already saved.");
            }

            var questionVariants = question.Variants.Where(x => questionVariantIds.Contains(x.Id)).ToList();

            return new Answer
            {
                QuestionId = question.Id,
                UserId = userId,
                QuestionVariants = questionVariants
            };
        }
    }
}
