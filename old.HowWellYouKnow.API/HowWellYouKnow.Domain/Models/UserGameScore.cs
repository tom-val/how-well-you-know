using System;
using System.Collections.Generic;
using System.Text;

namespace HowWellYouKnow.Domain.Models
{
    public class UserGameScore : BaseEntity
    {
        public User User { get; set; }
        public Guid UserId { get; set; }
        public int CurrentScore { get; set; }
        public GameState GameState { get; set; }
        public Guid GameStateId { get; set; }
    }
}
