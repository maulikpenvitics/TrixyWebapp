using Microsoft.AspNetCore.SignalR;
using Repository.FyersWebSocketServices;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Hubs
{
    public class StockHub:Hub
    {
        private static readonly ConcurrentDictionary<string, string> _activeUsers = new();
        public override Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                _activeUsers[userId] = userId; // Store the user ID globally
                Console.WriteLine($"User Connected: {userId}, Connection ID: {Context.ConnectionId}");
            }

            return base.OnConnectedAsync();
        }
        public async Task SendStockUpdate(List<StockData> stockData)
        {
            await Clients.All.SendAsync("ReceiveStockData", stockData);
        }
        public static string GetLoggedInUserId()
        {
            return _activeUsers.Keys.FirstOrDefault();
        }
    }
}
