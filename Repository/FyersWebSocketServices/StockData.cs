using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.FyersWebSocketServices
{
    public class StockData
    {
        public string? Symbol { get; set; }
        public decimal Price { get; set; }
        public decimal Change { get; set; }
        public Dictionary<string, decimal> Stocks { get; set; } = new Dictionary<string, decimal>();
    }
}
