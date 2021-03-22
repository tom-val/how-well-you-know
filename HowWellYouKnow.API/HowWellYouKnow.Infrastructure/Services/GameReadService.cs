using HowWellYouKnow.Domain.Dtos;
using HowWellYouKnow.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HowWellYouKnow.Infrastructure.Services
{
    public class GameReadService
    {
        private DatabaseContext databaseContext;
        public GameReadService(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public Task<GameStateDto> GetGameState(Guid gameId)
        {
             return databaseContext
                .Games
                .Where(x => x.Id == gameId)
                .Select(x => new GameStateDto
                {
                    GameId = x.Id,
                    CurrentGameState = x.GameState.CurrentGameState,
                    CurrentQuestion = x.GameState.CurrentQuestionId,
                    GameScores = x.GameState.GameScores.Select(s => new GameScoreDto
                    {
                        UserId = s.UserId,
                        CurrentScore = s.CurrentScore
                    }).ToList(),
                    AnswerResults = x.GameState.CurrentQuestion.AnswerResults.Select(r => new UserAnswerResultDto
                    {
                        UserId = r.UserId,
                        Correct = r.Correct,
                        AnswerVariants = r.AnswerQuestionVariants.Select(v => new QuestionVariantDto
                        {
                            Name = v.Name,
                            Notation = v.Notation,
                        }).ToList(),
                        GuessVariants = r.AnswerQuestionVariants.Select(v => new QuestionVariantDto
                        {
                            Name = v.Name,
                            Notation = v.Notation,
                        }).ToList(),
                    }).ToList()
                }).FirstOrDefaultAsync();
        }

        public Task<GameDto> GetGame(Guid gameId)
        {
            return databaseContext
               .Games
               .Where(x => x.Id == gameId)
               .Select(x => new GameDto
               {
                   GameId = x.Id,
                   Name = x.Name,
                   JoinedUsers = x.JoinedUsers.Select(u => new UserDto
                   {
                       Id = u.Id,
                       Name = u.Name
                   }).ToList(),
                   Questions = x.Questions.Select(q => new QuestionDto
                   {
                       Id = q.Id,
                       Name = q.Name,
                       MultipleAnswers = q.MultipleAnswers,
                       Variants = q.Variants.Select(v => new QuestionVariantDto
                       {
                           Id = v.Id,
                           Name = v.Name,
                           Notation = v.Notation,
                       }).ToList()
                   }).ToList()
               }).FirstOrDefaultAsync();
        }

        public Task<GameStatusDto> GetGameStatus(Guid gameId)
        {
            return databaseContext
               .Games
               .Where(x => x.Id == gameId)
               .Select(x => new GameStatusDto
               {
                   GameState = x.GameState.CurrentGameState,
                   JoinedUsers = x.JoinedUsers.Select(u => new UserDto
                   {
                       Id = u.Id,
                       Name = u.Name
                   }).ToList(),
               }).FirstOrDefaultAsync();
        }
    }
}
