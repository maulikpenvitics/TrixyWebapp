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
        public async Task SendStockUpdate(List<StockData> stockData)
        {
            await Clients.All.SendAsync("ReceiveStockData", stockData);
        }
    }
}
