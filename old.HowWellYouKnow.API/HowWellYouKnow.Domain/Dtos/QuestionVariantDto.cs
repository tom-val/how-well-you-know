using System;
using System.Collections.Generic;
using System.Text;

namespace HowWellYouKnow.Domain.Dtos
{
    public class QuestionVariantDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public char Notation { get; set; }
    }
}
