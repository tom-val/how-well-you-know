using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HowWellYouKnow.API.Requests
{
    public class UserGuessRequest
    {
        public Guid QuestionId { get; set; }
        public List<Guid> QuestionVariants { get; set; }
        public Guid GuessUser { get; set; }
    }
}
