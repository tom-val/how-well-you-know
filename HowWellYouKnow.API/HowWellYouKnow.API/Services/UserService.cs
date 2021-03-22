using HowWellYouKnow.API.Requests;
using HowWellYouKnow.Domain.Models;
using HowWellYouKnow.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HowWellYouKnow.API.Services
{
    public class UserService
    {
        private UserRepository userRepository;
        public UserService(UserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public async Task<Guid> CreateOrReturnUser(UserRequest request)
        {
            var user = await userRepository.GetUser(request.Name);

            if (user != null)
            {
                return user.Id;
            }

            user = new User
            {
                Name = request.Name
            };

            await userRepository.SaveUser(user);
            await userRepository.SaveChanges();

            return user.Id;
        }
    }
}
