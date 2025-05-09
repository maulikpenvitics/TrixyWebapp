using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.FyersWebSocketServices
{
    public class FyersStockSettings
    {
        public string? BaseUrlForhistory { get; set; }
        public string? ClientId { get; set; }
        public string? AccessToken { get; set; }
        public string? secretKey { get; set; }
        public string? redirectURI { get; set; }
        public string? Refgrant_type { get; set; }
        public string? Refrshtokenapi { get; set; }
        public string? grant_type { get; set; }
        public string? Apphasid { get; set; }
        public string? ValidateApi { get; set; }
        public string? refreshtoken { get; set; }
        public string? clinetpin { get; set; }
        public string? symbolquotes { get; set; }
    }
}
