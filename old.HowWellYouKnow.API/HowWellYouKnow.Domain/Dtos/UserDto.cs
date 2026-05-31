using System;
using System.Collections.Generic;
using System.Text;

namespace HowWellYouKnow.Domain.Dtos
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
    }
}
