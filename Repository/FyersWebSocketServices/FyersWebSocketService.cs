using FyersCSharpSDK;
using HyperSyncLib;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using Repository.FyersWebSocketServices;
using Repository.Hubs;
using Repository.IRepositories;
using Repository.Models;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        public  List<string> _stocklist=new();
    
        //private const string BaseUrl = "https://api-t1.fyers.in/data/history";
        private const string ClientId = "NGX016JVE9-100";
        private const string AccessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkuZnllcnMuaW4iLCJpYXQiOjE3NDEzMjcyMjUsImV4cCI6MTc0MTM5MzgyNSwibmJmIjoxNzQxMzI3MjI1LCJhdWQiOlsieDowIiwieDoxIiwieDoyIiwiZDoxIiwiZDoyIiwieDoxIiwieDowIl0sInN1YiI6ImFjY2Vzc190b2tlbiIsImF0X2hhc2giOiJnQUFBQUFCbnlvdDVVUWF0Q1h0NWpHaFhxeGlCRUViT21uLUJLN0xQbnh6aGFlSnZqakZCZVBLNUlXU09RQVZEd200UmYwNnVPWlBlWGY1aUdlZmZTNG1uTW9nWjVSOW5UQVB5N2F0MWV0RkUyNTUyd3hoenRmaz0iLCJkaXNwbGF5X25hbWUiOiJWQVJTSEFCRU4gTkFSQVlBTkJIQUkgREFCSEkiLCJvbXMiOiJLMSIsImhzbV9rZXkiOiIyMWIyNzc2MDEyNDk1ZmYwMzdlMDY5MTc3ZTQ2ODRkMmZjNTI2ZDNkODZhYjEzYjA3OGExNTc2MyIsImlzRGRwaUVuYWJsZWQiOiJOIiwiaXNNdGZFbmFibGVkIjoiTiIsImZ5X2lkIjoiWVYxNjU2OSIsImFwcFR5cGUiOjEwMCwicG9hX2ZsYWciOiJOIn0.im-BHe-x-4rNZDAalftZYMQYXBnXFv1sR1IimPmUQ7I";
      
      
        public FyersWebSocketService(IHubContext<StockHub> hubContext, HttpClient httpClient)
        {
            _hubContext = hubContext;
            _httpClient = httpClient;
        }
        public void LogFilegenerate(Exception ex, string path)
        {
            string errorMessage = $"DateTime: {DateTime.Now:dd/MM/yyyy hh:mm:ss tt}";
            errorMessage += Environment.NewLine;
            errorMessage += "------------------------Exception-----------------------------------";
            errorMessage += Environment.NewLine;
            errorMessage += $"Path: {path}";
            errorMessage += Environment.NewLine;
            errorMessage += $"Message: {ex.Message}";
            errorMessage += Environment.NewLine;
            errorMessage += $"Details: {ex}";
            errorMessage += Environment.NewLine;
            errorMessage += "-----------------------------------------------------------";
            errorMessage += Environment.NewLine;

            // Get the path to the "ErrorLog" directory in wwwroot
            string logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ErrorLog");

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            string logFilePath = Path.Combine(logDirectory, "WebSoketErrorLog.txt");

            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine(errorMessage);
            }
        }
        public List<StockData> GetStockData()
        {
            lock (_lock)
            {
                return _stockDataList.ToList();
            }
        }
        public List<string> GetStockList()
        {
            return _stocklist;
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
            string token = "NGX016JVE9-100:eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkuZnllcnMuaW4iLCJpYXQiOjE3NDEzMjcyMjUsImV4cCI6MTc0MTM5MzgyNSwibmJmIjoxNzQxMzI3MjI1LCJhdWQiOlsieDowIiwieDoxIiwieDoyIiwiZDoxIiwiZDoyIiwieDoxIiwieDowIl0sInN1YiI6ImFjY2Vzc190b2tlbiIsImF0X2hhc2giOiJnQUFBQUFCbnlvdDVVUWF0Q1h0NWpHaFhxeGlCRUViT21uLUJLN0xQbnh6aGFlSnZqakZCZVBLNUlXU09RQVZEd200UmYwNnVPWlBlWGY1aUdlZmZTNG1uTW9nWjVSOW5UQVB5N2F0MWV0RkUyNTUyd3hoenRmaz0iLCJkaXNwbGF5X25hbWUiOiJWQVJTSEFCRU4gTkFSQVlBTkJIQUkgREFCSEkiLCJvbXMiOiJLMSIsImhzbV9rZXkiOiIyMWIyNzc2MDEyNDk1ZmYwMzdlMDY5MTc3ZTQ2ODRkMmZjNTI2ZDNkODZhYjEzYjA3OGExNTc2MyIsImlzRGRwaUVuYWJsZWQiOiJOIiwiaXNNdGZFbmFibGVkIjoiTiIsImZ5X2lkIjoiWVYxNjU2OSIsImFwcFR5cGUiOjEwMCwicG9hX2ZsYWciOiJOIn0.im-BHe-x-4rNZDAalftZYMQYXBnXFv1sR1IimPmUQ7I"; // Replace with actual token
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
                    decimal precloseprice = scrip["prev_close_price"]?.ToObject<decimal>() ?? 0;

                    if (!string.IsNullOrEmpty(symbol))
                    {
                        var existingStock = _stockDataList.FirstOrDefault(s => s.Symbol == symbol);
                        if (existingStock != null)
                        {
                            existingStock.Price = price; 
                        }
                        else
                        {
                            _stockDataList.Add(new StockData { Symbol = symbol, Price = price ,prev_close_price=precloseprice});
                        }
                    }
                }
            }
            _hubContext.Clients.All.SendAsync("ReceiveStockData", _stockDataList);
        }

        public async void Connect(List<Stocks> stocks)
        {
            try
            {
                FyersClass fyersModel = FyersClass.Instance;
                fyersModel.ClientId = ClientId;
                fyersModel.AccessToken = AccessToken;
                _stocklist = stocks.Select(x => x.Symbol).ToList();
                Methods t = new Methods(this); // Pass the HubContext
                await t.DataWebSocket();
            }
            catch (Exception ex)
            {
                LogFilegenerate(ex, "FyersWebSocketService");
            }
            
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
            try
            {
                client = new FyersSocket();
                client.ReconnectAttemptsCount = 1;
                client.webSocketDelegate = this;

                await client.Connect();
                client.ConnectHSM(ChannelModes.FULL);

                var stocklist = _service.GetStockList();
                client.SubscribeData(stocklist);
            }
            catch (Exception ex)
            {

                _service.LogFilegenerate(ex, "FyersWebSocketService/Methods");
            }
          
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
            try
            {
                Console.WriteLine("WebSocket Error: " + error);
            }
            catch (Exception ex)
            {
                _service.LogFilegenerate(ex, "FyersWebSocketService/Methods"+ error);
            }
           
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
            try
            {
                Console.WriteLine("WebSocket Connection Opened: " + status);

            }
            catch (Exception ex)
            {
                _service.LogFilegenerate(ex, "FyersWebSocketService/Methods");
            }
           
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
            try
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
            catch (Exception ex)
            {

                _service.LogFilegenerate(ex, "FyersWebSocketService/Methods");
            }
           
        }


        public void OnTrade(JObject trades)
        {
            Console.WriteLine("Trade Data: " + trades);
        }
    }
}
