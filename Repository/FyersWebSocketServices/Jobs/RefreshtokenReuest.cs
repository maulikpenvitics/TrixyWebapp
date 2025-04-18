using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.FyersWebSocketServices.Jobs
{
    public class RefreshtokenReuest
    {
        public string? grant_type { get; set; }
        public string? appIdHash { get; set; }
        public string? refresh_token { get; set; }
        public string? pin { get; set; }
    }
    public class TokenResponse
    {
        public string? access_token { get; set; }
        public int code { get; set; }
        public string? message { get; set; }
        public string? s { get; set; }
    }
    public class Accesstoken
    {
        public string? TOKEN { get; set; }
        public string refresh_token { get; set; }
        public string? RESPONSE_MESSAGE { get; set; }
    }
   
}
