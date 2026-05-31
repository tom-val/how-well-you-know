using HowWellYouKnow.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HowWellYouKnow.Infrastructure.Repositories
{
    public class AnswerResultRepository
    {
        private DatabaseContext databaseContext;
        public AnswerResultRepository(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public async Task SaveAnswerResult(UserAnswerResult result)
        {
            await databaseContext.UserAnswerResults.AddAsync(result);
        }

        public async Task SaveAnswerResult(List<UserAnswerResult> results)
        {
            await databaseContext.UserAnswerResults.AddRangeAsync(results);
        }

        public async Task SaveChanges()
        {
            await databaseContext.SaveChangesAsync();
        }
    }
}
