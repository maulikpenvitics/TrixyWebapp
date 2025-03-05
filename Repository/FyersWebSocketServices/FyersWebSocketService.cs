using FyersCSharpSDK;
using HyperSyncLib;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Repository.FyersWebSocketServices;
using Repository.Hubs;
using Repository.Models;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.Json;
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
        //private const string BaseUrl = "https://api-t1.fyers.in/data/history";
        private const string ClientId = "NGX016JVE9-100";
        private const string AccessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkubG9naW4uZnllcnMuaW4iLCJpYXQiOjE3NDEwOTI3OTcsImV4cCI6MTc0MTEyMjc5NywibmJmIjoxNzQxMDkyMTk3LCJhdWQiOiJbXCJ4OjBcIiwgXCJ4OjFcIiwgXCJ4OjJcIiwgXCJkOjFcIiwgXCJkOjJcIiwgXCJ4OjFcIiwgXCJ4OjBcIl0iLCJzdWIiOiJhdXRoX2NvZGUiLCJkaXNwbGF5X25hbWUiOiJZVjE2NTY5Iiwib21zIjoiSzEiLCJoc21fa2V5IjoiMjFiMjc3NjAxMjQ5NWZmMDM3ZTA2OTE3N2U0Njg0ZDJmYzUyNmQzZDg2YWIxM2IwNzhhMTU3NjMiLCJpc0RkcGlFbmFibGVkIjoiTiIsImlzTXRmRW5hYmxlZCI6Ik4iLCJub25jZSI6IiIsImFwcF9pZCI6Ik5HWDAxNkpWRTkiLCJ1dWlkIjoiNWQwNmU5MGE2MTRmNDBmOTk1Y2NjNWQ5NDc3ZDk4YWYiLCJpcEFkZHIiOiIwLjAuMC4wIiwic2NvcGUiOiIifQ.BEnMDhnkg9AC2t1UeOU_xcxnbHmEOTMoZDfFwP5clIY";
       
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
        public async Task<List<Historical_Data>> FetchAndStoreHistoricalStockDataAsync()
        {
            List<Historical_Data> stockHistorylist = new List<Historical_Data>();
            string? symbol = "NSE:OFSS-EQ"; // Ensure the correct format
            string? rangefrom = "2025-01-01";
            string? rangeto = "2025-03-04";
            string apiUrl = "https://api-t1.fyers.in/data/history?symbol=NSE:OFSS-EQ&resolution=240&date_format=1&range_from=2025-01-01&range_to=2025-03-04";
            //string apiUrl = $"https://api-t1.fyers.in/data/history?symbol={symbol}&resolution=15&date_format=1&range_from={rangefrom}&range_to={rangeto}";

            using var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);

            // Add Authorization Token
            string token = "NGX016JVE9-100:eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkuZnllcnMuaW4iLCJpYXQiOjE3NDEwOTMzNDYsImV4cCI6MTc0MTEzNDYyNiwibmJmIjoxNzQxMDkzMzQ2LCJhdWQiOlsieDowIiwieDoxIiwieDoyIiwiZDoxIiwiZDoyIiwieDoxIiwieDowIl0sInN1YiI6ImFjY2Vzc190b2tlbiIsImF0X2hhc2giOiJnQUFBQUFCbnh2bmlXc1dRWFpFdW1ZVm1WakUxTUM0Q3h6czQ4S1JmZEFqY0w5aUYycGRZWlJEclFGNVRGMTk0bkR0TDlzUXpRdDc3X3hmQ21SYTk1MFdNd09ucThIdDQwMGlidnByYVNrUHdwalYtZnVUdEdLTT0iLCJkaXNwbGF5X25hbWUiOiJWQVJTSEFCRU4gTkFSQVlBTkJIQUkgREFCSEkiLCJvbXMiOiJLMSIsImhzbV9rZXkiOiIyMWIyNzc2MDEyNDk1ZmYwMzdlMDY5MTc3ZTQ2ODRkMmZjNTI2ZDNkODZhYjEzYjA3OGExNTc2MyIsImlzRGRwaUVuYWJsZWQiOiJOIiwiaXNNdGZFbmFibGVkIjoiTiIsImZ5X2lkIjoiWVYxNjU2OSIsImFwcFR5cGUiOjEwMCwicG9hX2ZsYWciOiJOIn0.ndFCo13-f5TQpgCrV5sxsVkw47Pmq_-HIRb70vJTjpc"; // Replace with actual token
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            using var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonSerializer.Deserialize<HistoryStockdataRoot>(responseString);

                if (jsonResponse != null && jsonResponse.candles!=null && jsonResponse.candles.Count>0 && jsonResponse.s== "ok")
                {
                    foreach (var item in jsonResponse.candles)
                    {
                        var stockHistory = new Historical_Data();
                        long timestamp = Convert.ToInt64(item[0]);
                        DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
                        stockHistory.Timestamp = dateTime;
                        stockHistory.Open = Convert.ToDecimal(item[1]);
                        stockHistory.High = Convert.ToDecimal(item[2]);
                        stockHistory.Low = Convert.ToDecimal(item[3]);
                        stockHistory.Close = Convert.ToDecimal(item[4]);
                        stockHistory.Volume = Convert.ToDecimal(item[5]);
                        stockHistory.from_date = rangefrom;
                        stockHistory.to_date = rangeto;
                        stockHistory.symbol = symbol;
                        stockHistorylist.Add(stockHistory);
                    }
                }
            }
            else
            {
                Console.WriteLine($"API Error: {response.StatusCode}");
            }
            return stockHistorylist;
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
                    string? symbol = scrip["symbol"]?.ToString();
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
