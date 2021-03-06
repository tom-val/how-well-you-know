using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HowWellYouKnow.Domain.Models;
using HowWellYouKnow.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HowWellYouKnow.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        DatabaseContext context;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, DatabaseContext context)
        {
            _logger = logger;
            this.context = context;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            var gameGuid = Guid.NewGuid();
            var userId = Guid.NewGuid();

            context.Users.Add(new User
            {
                Id = userId,
                Name = "Tomas"
            });

            context.Games.Add(new Game
            {
                Id = gameGuid,
                CreatedByUserId = userId,
                GameState = new GameState
                {
                    CurrentGameState = Domain.Enums.CurrentGameState.NotStarted
                }
            });

            context.Questions.Add(new Question
            {
                Name = "TEST",
                MultipleAnswers = true,
                GameId = gameGuid
            });
            context.SaveChanges();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
