using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HowWellYouKnow.API.Requests;
using HowWellYouKnow.API.Services;
using HowWellYouKnow.Domain.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HowWellYouKnow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private UserService userService;
        public UserController(UserService userService)
        {
            this.userService = userService;
        }

        [HttpPost]
        public Task<Guid> CreateGet(UserRequest request)
        {
            return userService.CreateOrReturnUser(request);
        }

        [HttpGet("{id}/name")]
        public Task<UserDto> GetUserName([FromRoute] Guid id)
        {
            return userService.GetUserName(id);
        }
    }
}