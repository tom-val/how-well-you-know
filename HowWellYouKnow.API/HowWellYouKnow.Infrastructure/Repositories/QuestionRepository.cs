using HowWellYouKnow.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HowWellYouKnow.Infrastructure.Repositories
{
    public class QuestionRepository
    {
        private DatabaseContext databaseContext;
        public QuestionRepository(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public async Task SaveQuestion(Question question)
        {
            await databaseContext.Questions.AddAsync(question);
        }

        public Task<Question> GetQuestion(Guid questionId)
        {
            return databaseContext.Questions.Include(x => x.Variants)
                .FirstOrDefaultAsync(x => x.Id == questionId);
        }

        public async Task SaveChanges()
        {
            await databaseContext.SaveChangesAsync();
        }
    }
}
