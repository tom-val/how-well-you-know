using System;
using System.Collections.Generic;
using System.Text;

namespace HowWellYouKnow.Domain.Models
{
    public class Question : BaseEntity
    {
        public string Name { get; set; }
        public bool MultipleAnswers { get; set; }
        public List<QuestionVariant> Variants { get; set; }
        public List<Guess> Guesses { get; set; }
        public List<UserAnswerResult> AnswerResults { get; set; }
        public List<Answer> Answers { get; set; }
        public Guid GameId { get; set; }
        public Game Game { get; set; }
        public Guid? GameStateId { get; set; }
        public GameState GameState { get; set; }
    }
}
