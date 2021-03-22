using HowWellYouKnow.Domain.Dtos;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HowWellYouKnow.API.Hubs
{
    public class QuestionsHub : Hub
    {
        public async Task SendMessage(string gameId, QuestionDto question)
        {
           await Clients.All.SendAsync(gameId, question);
        }
    }
}
