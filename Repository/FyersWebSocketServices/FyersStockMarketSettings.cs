using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.FyersWebSocketServices
{
    public class FyersStockMarketSettings
    {
        public string? BaseUrl { get; set; }
        public string? ClientId { get; set; }
        public string? AccessToken { get; set; }
    }
    public class QuoteResponse
    {
        public string message { get; set; }
        public int code { get; set; }
        public List<QuoteData> d { get; set; }
        public string s { get; set; }
    }

    public class QuoteData
    {
        public string n { get; set; }
        public QuoteValue v { get; set; }
        public string s { get; set; }
    }

    public class QuoteValue
    {
        public double ask { get; set; }
        public double bid { get; set; }
        public double chp { get; set; }
        public double ch { get; set; }
        public string description { get; set; }
        public string exchange { get; set; }
        public string fyToken { get; set; }
        public double high_price { get; set; }
        public double low_price { get; set; }
        public double lp { get; set; }
        public double open_price { get; set; }
        public string original_name { get; set; }
        public double prev_close_price { get; set; }
        public string short_name { get; set; }
        public double spread { get; set; }
        public string symbol { get; set; }
        public string tt { get; set; }  // You can convert this to long or DateTime if needed
        public int volume { get; set; }
        public double atp { get; set; }
        public string errmsg { get; set; }
        public int code { get; set; }
        public string s { get; set; }
    }
}
