using System;
using System.Collections.Generic;
using System.Text;

namespace HowWellYouKnow.Domain.Models
{
    public class Question : BaseEntity
    {
        public string Name { get; set; }
        public List<QuestionVariant> Variants { get; set; }
    }
}
