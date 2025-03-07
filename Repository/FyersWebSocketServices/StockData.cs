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
        public decimal prev_close_price { get; set; }
        public decimal Change
        {
            get
            {
                return prev_close_price != 0
                ? Math.Round(((Price - prev_close_price) / prev_close_price) * 100, 2)
                : 0;
            }
        }
        public Dictionary<string, decimal> Stocks { get; set; } = new Dictionary<string, decimal>();
    }
}
