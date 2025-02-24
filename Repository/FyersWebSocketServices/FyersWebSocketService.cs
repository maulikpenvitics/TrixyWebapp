using FyersCSharpSDK;
using HyperSyncLib;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Repository.FyersWebSocketServices;
using Repository.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace Repository.FyersWebSocketServices
{
    public class FyersWebSocketService
    {
        private readonly List<StockData> _stockDataList = new();
        private readonly object _lock = new();
        private readonly IHubContext<StockHub> _hubContext;
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api-t1.fyers.in/data/history";
        private const string ClientId = "NGX016JVE9-100";
        private const string AccessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkuZnllcnMuaW4iLCJpYXQiOjE3NDAxMTQ5NDcsImV4cCI6MTc0MDE4NDI0NywibmJmIjoxNzQwMTE0OTQ3LCJhdWQiOlsieDowIiwieDoxIiwieDoyIiwiZDoxIiwiZDoyIiwieDoxIiwieDowIl0sInN1YiI6ImFjY2Vzc190b2tlbiIsImF0X2hhc2giOiJnQUFBQUFCbnVBd0Riak9ObElNUkxTOUpzQVBWM1ZhYmdTclNaUl9wa2RLay0tcmpjcEZJcXpaWlpUWTdMejR0S3hXZEtVckdlNk00djZubm5HT2hwTzdIckJtXzNSQnlKR3BNUXhBSDJNNk55RWlPRDlpVUlOTT0iLCJkaXNwbGF5X25hbWUiOiJWQVJTSEFCRU4gTkFSQVlBTkJIQUkgREFCSEkiLCJvbXMiOiJLMSIsImhzbV9rZXkiOiIyMWIyNzc2MDEyNDk1ZmYwMzdlMDY5MTc3ZTQ2ODRkMmZjNTI2ZDNkODZhYjEzYjA3OGExNTc2MyIsImlzRGRwaUVuYWJsZWQiOiJOIiwiaXNNdGZFbmFibGVkIjoiTiIsImZ5X2lkIjoiWVYxNjU2OSIsImFwcFR5cGUiOjEwMCwicG9hX2ZsYWciOiJOIn0.at2WFICBwvkS4uG679d8FGoE5-MEKwZjdSSlsNBpiac";
        private readonly FyersStockMarketSettings _settings;
        public FyersWebSocketService(IHubContext<StockHub> hubContext, HttpClient httpClient)
        {
            _hubContext = hubContext;
            _httpClient = httpClient;
            
        }
        public List<StockData> GetStockData()
        {
            lock (_lock)
            {
                return _stockDataList.ToList();
            }
        }

        public void UpdateStockData(JToken scripsToken)
        {
            lock (_lock)
            {
                JArray scripsArray = scripsToken is JObject singleScrip
                    ? new JArray { singleScrip }
                    : (JArray)scripsToken;

                var stockData = new StockData();

                foreach (var scrip in scripsArray)
                {
                    string symbol = scrip["symbol"]?.ToString();
                    decimal price = scrip["ltp"]?.ToObject<decimal>() ?? 0;

                    if (!string.IsNullOrEmpty(symbol))
                    {
                        var existingStock = _stockDataList.FirstOrDefault(s => s.Symbol == symbol);
                        if (existingStock != null)
                        {
                            existingStock.Price = price; // Update price
                        }
                        else
                        {
                            _stockDataList.Add(new StockData { Symbol = symbol, Price = price });
                        }
                    }
                }
            }
            _hubContext.Clients.All.SendAsync("ReceiveStockData", _stockDataList);
        }

        public async void Connect()
        {
            FyersClass fyersModel = FyersClass.Instance;
            fyersModel.ClientId = ClientId;
            fyersModel.AccessToken = AccessToken; 

            Methods t = new Methods(this); // Pass the HubContext
            await t.DataWebSocket();
        }
    }
    public class Methods : FyersSocketDelegate
    {
        private readonly IHubContext<StockHub> _hubContext;
        private readonly FyersWebSocketService _service;
        private FyersSocket client;

        public Methods(FyersWebSocketService service)
        {
            _service = service;
        }

        public async Task DataWebSocket()
        {
            client = new FyersSocket();
            client.ReconnectAttemptsCount = 1;
            client.webSocketDelegate = this;

            await client.Connect();
            client.ConnectHSM(ChannelModes.FULL);

            List<string> scripList = new List<string>
            {
                "NSE:ITC-EQ",
                "NSE:RELIANCE-EQ",
                "NSE:BAJFINANCE-EQ",
                "NSE:ABFRL-EQ",
                "NSE:HINDUNILVR-EQ",
                "NSE:ICICIBANK-EQ",
                "NSE:IDFCFIRSTB-EQ",
                "NSE:INFY-EQ",
                "NSE:TCS-EQ",
                "NSE:VEDL-EQ"
            };
            client.SubscribeData(scripList);
        }

        public void OnClose(string status)
        {
            Console.WriteLine("Connection closed: " + status);
        }

        public void OnDepth(JObject depths)
        {
            Console.WriteLine("Market Depth Data: " + depths);
        }

        public void OnError(JObject error)
        {
            Console.WriteLine("WebSocket Error: " + error);
        }

        public void OnIndex(JObject index)
        {
            Console.WriteLine("Index Data: " + index);
        }

        public void OnMessage(JObject response)
        {
            Console.WriteLine("OnMessage: " + response);
        }

        public void OnOpen(string status)
        {
            Console.WriteLine("WebSocket Connection Opened: " + status);
        }

        public void OnOrder(JObject orders)
        {
            Console.WriteLine("Order Data: " + orders);
        }

        public void OnPosition(JObject positions)
        {
            Console.WriteLine("Position Data: " + positions);
        }

        public async void OnScrips(JObject scrips)
        {
            Console.WriteLine("Received Scrip Data: " + scrips);

            var data = scrips["data"];

            if (data is JObject singleStock)
            {
                var scripsArray = new JArray { singleStock };
                _service.UpdateStockData(scripsArray);
            }
            else if (data is JArray scripsArray)
            {
                _service.UpdateStockData(scripsArray);
            }
            else
            {
                Console.WriteLine("Invalid data format received.");
            }
        }


        public void OnTrade(JObject trades)
        {
            Console.WriteLine("Trade Data: " + trades);
        }
    }
}
