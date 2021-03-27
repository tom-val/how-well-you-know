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
                    LastQuestionId = x.LastQuestionId,
                    GameScores = x.GameState.GameScores.Select(s => new GameScoreDto
                    {
                        UserId = s.UserId,
                        Name = s.User.Name,
                        CurrentScore = s.CurrentScore
                    }).ToList(),
                    AnswerResults = x.GameState.CurrentQuestion.AnswerResults.Select(r => new UserAnswerResultDto
                    {
                        UserId = r.UserId,
                        Name = r.User.Name,
                        Correct = r.Correct,
                        GuessName = r.GuessUser.Name,
                        AnswerVariants = r.AnswerQuestionVariants.Select(v => new QuestionVariantDto
                        {
                            Name = v.Name,
                            Notation = v.Notation,
                        }).OrderBy(x => x.Notation).ToList(),
                        GuessVariants = r.GuessQuestionVariants.Select(v => new QuestionVariantDto
                        {
                            Name = v.Name,
                            Notation = v.Notation,
                        }).OrderBy(x => x.Notation).ToList(),
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
                   LastQuestionId = x.LastQuestionId,
                   JoinedUsers = x.JoinedUsers.Select(u => new UserDto
                   {
                       Id = u.Id,
                       Name = u.Name
                   }).ToList(),
                   Questions = x.Questions.Select(q => new QuestionDto
                   {
                       Id = q.Id,
                       Name = q.Name,
                       Order = q.Order,
                       MultipleAnswers = q.MultipleAnswers,
                       Variants = q.Variants.Select(v => new QuestionVariantDto
                       {
                           Id = v.Id,
                           Name = v.Name,
                           Notation = v.Notation,
                       }).OrderBy(x => x.Notation).ToList()
                   }).OrderBy(x => x.Order).ToList()
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
