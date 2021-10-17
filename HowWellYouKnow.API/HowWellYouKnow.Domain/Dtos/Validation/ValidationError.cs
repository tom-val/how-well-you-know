using System;
using System.Collections.Generic;
using System.Text;

namespace HowWellYouKnow.Domain.Dtos.Validation
{
    public class ValidationError
    {
        public string Field { get; set; }
        public string Error { get; set; }
    }
}
