using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver.Core.Connections;
using Repository.FyersWebSocketServices;
using Repository.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Hubs
{
    public class StockNotificationHub:Hub
    {
        private static readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();
        private static readonly ConcurrentDictionary<string, string> _activeUsers = new();
        public override Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections.AddOrUpdate(
                    userId,
                    new HashSet<string> { Context.ConnectionId }, // Create new entry if user doesn't exist
                    (key, existingConnections) =>
                    {
                        existingConnections.Add(Context.ConnectionId);
                        return existingConnections;
                    });
                _activeUsers[userId] = userId; // Store the user ID globally
                Console.WriteLine($"User Connected: {userId}, Connection ID: {Context.ConnectionId}");
            }

            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                if (_userConnections.TryGetValue(userId, out var connections))
                {
                    connections.Remove(Context.ConnectionId);
                    if (connections.Count == 0)
                    {
                        _userConnections.TryRemove(userId, out _);
                        _activeUsers.TryRemove(userId, out _);
                    }
                }
            }

            return base.OnDisconnectedAsync(exception);
        }
        // Method to get online users
        public Task GetOnlineUsers()
        {
            return Clients.Caller.SendAsync("ReceiveOnlineUsers", _userConnections.Keys);
        }
        public static string GetLoggedInUserId()
        {
            var activevuser =_activeUsers.Keys.FirstOrDefault();
            return activevuser!=null?activevuser:string.Empty;
        }
        public static List<string> GetConnectedUsers()
        {
            return _userConnections.Keys.ToList();
        }
        public static bool TryGetConnections(string userId, out HashSet<string> connections)
        {
            return _userConnections.TryGetValue(userId, out connections);
        }

        // Send stock updates to a specific user
        public async Task SendStockUpdateToUser(string userId, List<Stocknotifactiondata> stocknotifactiondatas)
        {
            // await Clients.All.SendAsync("ReceiveStockUpdate", stocknotifactiondatas);
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                foreach (var connectionId in connections)
                {
                    await Clients.Client(connectionId).SendAsync("ReceiveStockUpdate", stocknotifactiondatas);
                }
            }
        }
    }
}
