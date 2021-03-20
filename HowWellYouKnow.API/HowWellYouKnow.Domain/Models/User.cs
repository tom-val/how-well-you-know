using System;
using System.Collections.Generic;
using System.Text;

namespace HowWellYouKnow.Domain.Models
{
    public class User: BaseEntity
    {
        public string Name { get; set; }
        public DateTime Created { get; set; }
    }
}
