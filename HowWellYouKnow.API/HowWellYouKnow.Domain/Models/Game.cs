using System;
using System.Collections.Generic;
using System.Text;

namespace HowWellYouKnow.Domain.Models
{
    public class Game: BaseEntity
    {
        public string Name { get; set; }
        public Guid CreatedByUserId { get; set; }
        public User CreatedBy { get; set; }
        public List<Question> Questions { get; set; }
        public Guid GameStateId { get; set; }
        public GameState GameState { get; set; }
        public List<User> JoinedUsers { get; set; }
    }
}
