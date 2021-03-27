using System;
using System.Collections.Generic;
using System.Text;

namespace HowWellYouKnow.Domain.Dtos
{
    public class GameDto
    {
        public Guid GameId { get; set; }
        public string Name { get; set; }
        public Guid? LastQuestionId { get; set; }
        public List<UserDto> JoinedUsers { get; set; }
        public List<QuestionDto> Questions { get; set; }
    }

    public class QuestionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public bool MultipleAnswers { get; set; }
        public List<QuestionVariantDto> Variants { get; set; }
    }
}
