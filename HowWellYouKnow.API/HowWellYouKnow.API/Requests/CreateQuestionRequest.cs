using HowWellYouKnow.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HowWellYouKnow.API.Requests
{
    public class CreateQuestionRequest
    {
        public string Name { get; set; }
        public bool MultipleAnswers { get; set; }
        public List<QuestionVariantDto> Variants { get; set; }
    }
}
