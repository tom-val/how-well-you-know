using HowWellYouKnow.API.Hubs;
using HowWellYouKnow.API.Requests;
using HowWellYouKnow.Domain.Dtos;
using HowWellYouKnow.Domain.Models;
using HowWellYouKnow.Infrastructure.Repositories;
using Microsoft.AspNetCore.SignalR;
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

        public async Task<UserDto> GetUserName(Guid userId)
        {
            var user = await userRepository.GetUser(userId);
            return new UserDto
            {
                Id = user.Id,
                Name = user.Name
            };
        }
    }
}
