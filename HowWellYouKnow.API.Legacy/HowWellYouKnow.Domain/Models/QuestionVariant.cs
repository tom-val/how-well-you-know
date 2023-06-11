using System;
using System.Collections.Generic;
using System.Text;

namespace HowWellYouKnow.Domain.Models
{
    public class QuestionVariant : BaseEntity
    {
        public string Name { get; set; }
        public char Notation { get; set; }
        public Guid QuestionId { get; set; }
        public Question Question { get; set; }
        public List<Guess> Guesses { get; set; }
        public List<UserAnswerResult> AnswerResults { get; set; }
        public List<UserAnswerResult> GuessResults { get; set; }
        public List<Answer> Answers { get; set; }
    }
}
