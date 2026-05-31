using HowWellYouKnow.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HowWellYouKnow.Infrastructure.Repositories
{
    public class AnswerRepository
    {
        private DatabaseContext databaseContext;
        public AnswerRepository(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public async Task SaveAnswer(Answer answer)
        {
            await databaseContext.Answers.AddAsync(answer);
        }

        public async Task SaveChanges()
        {
            await databaseContext.SaveChangesAsync();
        }
    }
}
