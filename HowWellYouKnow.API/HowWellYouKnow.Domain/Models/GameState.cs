using HowWellYouKnow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HowWellYouKnow.Domain.Models
{
    public class GameState: BaseEntity
    {
        public Game Game { get; set; }
        public CurrentGameState CurrentGameState { get; set; }
        public Guid? CurrentQuestionId { get; set; }
        public Question CurrentQuestion { get; set; }
        public List<UserGameScore> GameScores { get; set; }
    }
}
