using System;
using System.Collections.Generic;
using System.Text;

namespace HowWellYouKnow.Domain.Models
{
    public class QuestionVariant : BaseEntity
    {
        public string Name { get; set; }
        public Guid QuestionId { get; set; }
        public Question Question { get; set; }
    }
}
