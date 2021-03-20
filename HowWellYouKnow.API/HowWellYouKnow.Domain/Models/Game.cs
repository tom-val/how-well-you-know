using System;
using System.Collections.Generic;
using System.Text;

namespace HowWellYouKnow.Domain.Models
{
    public class Game: BaseEntity
    {
        public Guid CreatedByUserId { get; set; }
        public User CreatedBy { get; set; }
    }
}
