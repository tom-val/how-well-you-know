using System;
using System.Collections.Generic;
using System.Text;

namespace HowWellYouKnow.Domain.Models
{
    public class Guess : BaseEntity
    {
        public Guid QuestionId { get; set; }
        public Question Question { get; set; }
        public List<QuestionVariant> QuestionVariants { get; set; }
        public User User { get; set; }
        public Guid UserId { get; set; }
        public User GuessUser { get; set; }
        public Guid GuessUserId { get; set; }
    }
}
