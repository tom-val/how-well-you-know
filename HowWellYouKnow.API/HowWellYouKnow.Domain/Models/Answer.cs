using System;
using System.Collections.Generic;

namespace HowWellYouKnow.Domain.Models
{
    public class Answer : BaseEntity
    {
        public Guid QuestionId { get; set; }
        public Question Question { get; set; }
        public List<QuestionVariant> QuestionVariants { get; set; }
        public User User { get; set; }
        public Guid UserId { get; set; }
    }
}
