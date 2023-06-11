using HowWellYouKnow.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HowWellYouKnow.Infrastructure.Repositories
{
    public class UserRepository
    {
        private DatabaseContext databaseContext;
        public UserRepository(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public Task<User> GetUser(string name)
        {
            return databaseContext.Users.FirstOrDefaultAsync(x => x.Name == name);
        }

        public Task<User> GetUser(Guid id)
        {
            return databaseContext.Users.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task SaveUser(User user)
        {
            await databaseContext.Users.AddAsync(user);
        }

        public async Task SaveChanges()
        {
            await databaseContext.SaveChangesAsync();
        }
    }


}
