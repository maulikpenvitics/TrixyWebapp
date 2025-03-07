using Repository.Models;

namespace TrixyWebapp.ViewModels
{
    public class StockDataViewModel
    {
        public string? Symbol { get; set; } 
        public decimal Price { get; set; } 
        public decimal prev_close_price { get; set; }
        public decimal Change { get; set; }
        public List<Stocks>? Stocks { get; set; }
    }
}
