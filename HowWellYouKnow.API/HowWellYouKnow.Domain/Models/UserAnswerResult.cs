using System;
using System.Collections.Generic;

namespace HowWellYouKnow.Domain.Models
{
    public class UserAnswerResult : BaseEntity
    {
        public Guid QuestionId { get; set; }
        public Question Question { get; set; }
        public List<QuestionVariant> AnswerQuestionVariants { get; set; }
        public List<QuestionVariant> GuessQuestionVariants { get; set; }
        public bool Correct { get; set; }
        public User User { get; set; }
        public Guid UserId { get; set; }
    }
}
