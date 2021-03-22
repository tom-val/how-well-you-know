﻿using HowWellYouKnow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HowWellYouKnow.Domain.Dtos
{
    public class GameStateDto
    {
        public Guid GameId { get; set; }
        public CurrentGameState CurrentGameState { get; set; }
        public Guid? CurrentQuestion { get; set; }
        public List<GameScoreDto> GameScores { get; set; }
        public List<UserAnswerResultDto> AnswerResults { get; set; }
    }

    public class GameScoreDto
    {
        public Guid UserId { get; set; }
        public int CurrentScore { get; set; }
    }

    public class UserAnswerResultDto
    {
        public List<QuestionVariantDto> AnswerVariants { get; set; }
        public List<QuestionVariantDto> GuessVariants { get; set; }
        public bool Correct { get; set; }
        public Guid UserId { get; set; }
    }
}
