using HowWellYouKnow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HowWellYouKnow.Domain.Dtos
{
    public class GameStatusDto
    {
        public List<UserDto> JoinedUsers { get; set; }
        public CurrentGameState GameState { get; set; }
    }
}
