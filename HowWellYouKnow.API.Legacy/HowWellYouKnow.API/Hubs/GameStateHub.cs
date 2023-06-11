using HowWellYouKnow.Domain.Dtos;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HowWellYouKnow.API.Hubs
{
    public class GameStateHub : Hub
    {
        public async Task SendMessage(string gameId, GameStateDto gameState)
        {
           await Clients.All.SendAsync(gameId, gameState);
        }
    }
}
