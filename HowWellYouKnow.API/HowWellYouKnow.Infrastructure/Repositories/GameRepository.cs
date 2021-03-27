using HowWellYouKnow.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HowWellYouKnow.Infrastructure.Repositories
{
    public class GameRepository
    {
        private DatabaseContext databaseContext;
        public GameRepository(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public async Task SaveGame(Game game)
        {
            await databaseContext.Games.AddAsync(game);
        }

        public Task<Game> GetGame(Guid gameId)
        {
            return databaseContext.Games
                .Include(x => x.JoinedUsers)
                .Include(x => x.GameState)
                .Include(x => x.Questions)
                .FirstOrDefaultAsync(x => x.Id == gameId);
        }

        public Task<Game> GetGameWithQuestions(Guid gameId)
        {
            return databaseContext.Games
                .Include(x => x.Questions)
                .FirstOrDefaultAsync(x => x.Id == gameId);
        }

        public Task<Game> GetGameWithGameState(Guid gameId)
        {
            return databaseContext.Games
                .Include(x => x.JoinedUsers)
                .Include(x => x.GameState)
                .Include(x => x.GameState.CurrentQuestion)
                .Include(x => x.GameState.GameScores)
                .Include(x => x.GameState.CurrentQuestion.Guesses)
                .ThenInclude(x => x.QuestionVariants)
                .Include(x => x.GameState.CurrentQuestion.Answers)
                .ThenInclude(x => x.QuestionVariants)
                .FirstOrDefaultAsync(x => x.Id == gameId);
        }

        public void UpdateGame(Game game)
        {
            databaseContext.Games.Update(game);
        }

        public async Task SaveChanges()
        {
            await databaseContext.SaveChangesAsync();
        }
    }
}
