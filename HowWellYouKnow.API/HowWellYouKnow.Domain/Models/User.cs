using System;
using System.Collections.Generic;
using System.Text;

namespace HowWellYouKnow.Domain.Models
{
    public class User: BaseEntity
    {
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public List<Game> CreatedGames { get; set; }
        public List<Game> JoinedGames { get; set; }
        public List<Guess> Guesses { get; set; }
        public List<Guess> GuessFor { get; set; }
        public List<UserAnswerResult> AnswerResults { get; set; }
        public List<Answer> Answers { get; set; }
        public List<UserGameScore> GameScores { get; set; }
    }
}
