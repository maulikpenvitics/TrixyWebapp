using FyersCSharpSDK;
using HyperSyncLib;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Repository.FyersWebSocketServices;
using Repository.Hubs;
using Repository.IRepositories;
using Repository.Models;
using SharpCompress.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Repository.FyersWebSocketServices
{
    public class FyersWebSocketService
    {
        private readonly List<StockData> _stockDataList = new List<StockData>();
        private readonly object _lock = new();
        private readonly IHubContext<StockHub> _hubContext;
        private readonly HttpClient _httpClient;
        public readonly List<string?> _stocklist=new List<string?>();
        private readonly IServiceProvider _serviceProvider;
        //private const string BaseUrl = "https://api-t1.fyers.in/data/history";
        private const string ClientId = "NGX016JVE9-100";
        private const string AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOlsiZDoxIiwiZDoyIiwieDowIiwieDoxIiwieDoyIl0sImF0X2hhc2giOiJnQUFBQUFCbjRRZ3VLQWk3Z29xQm5lZ3RUS0hiX1g2RllObHFKclVTLW9lMHQyZXg3Ui1JNjloTFRUWUtkclRSN1hhX2NxWXZ1MzMyaTlNTERpNDRuNVZwYlVWMW43dGMtaUpRdTBzWmVZblJpWlpxMWlBdTRIQT0iLCJkaXNwbGF5X25hbWUiOiIiLCJvbXMiOiJLMSIsImhzbV9rZXkiOiIyMWIyNzc2MDEyNDk1ZmYwMzdlMDY5MTc3ZTQ2ODRkMmZjNTI2ZDNkODZhYjEzYjA3OGExNTc2MyIsImlzRGRwaUVuYWJsZWQiOiJOIiwiaXNNdGZFbmFibGVkIjoiTiIsImZ5X2lkIjoiWVYxNjU2OSIsImFwcFR5cGUiOjEwMCwiZXhwIjoxNzQyODYyNjAwLCJpYXQiOjE3NDI4MDA5NDIsImlzcyI6ImFwaS5meWVycy5pbiIsIm5iZiI6MTc0MjgwMDk0Miwic3ViIjoiYWNjZXNzX3Rva2VuIn0.q6gG-CQMkYXI14uluXuH4M2a31s-P5nZuFrSjV1jpRI";
      
      
        public FyersWebSocketService(IHubContext<StockHub> hubContext, HttpClient httpClient, IServiceProvider serviceProvider)
        {
            _hubContext = hubContext;
            _httpClient = httpClient;
            _serviceProvider = serviceProvider;
        }
        public void LogFilegenerate(Exception? ex, string path)
        {
            string errorMessage = $"DateTime: {DateTime.Now:dd/MM/yyyy hh:mm:ss tt}";
            errorMessage += Environment.NewLine;
            errorMessage += "------------------------Exception-----------------------------------";
            errorMessage += Environment.NewLine;
            errorMessage += $"Path: {path}";
            errorMessage += Environment.NewLine;
            if (ex !=null)
            {
                errorMessage += $"Message: {ex.Message}";
                errorMessage += Environment.NewLine;
                errorMessage += $"Details: {ex}";
            }
        
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
        public List<StockData> GetStockData(List<Stocks>? stocks)
        {
            if (stocks != null)
            {
                var list = stocks.Select(x => x.Symbol).ToList();
                _stocklist.AddRange(list);
                _stocklist.ToList();
            }

            lock (_lock)
            {
                return _stockDataList.Where(x=> _stocklist.Contains(x.Symbol)).ToList();
            }
        }
        public async Task<List<Historical_Data>> FetchAndStoreHistoricalStockDataAsync(string sym,string rangform,string rangeto)
        {
            List<Historical_Data> stockHistorylist = new List<Historical_Data>();
           
            string apiUrl = $"https://api-t1.fyers.in/data/history?symbol={sym}&resolution=240&date_format=1&range_from={rangform}&range_to={rangeto}";

            using var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);

            // Add Authorization Token
            string token = "NGX016JVE9-100:eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOlsiZDoxIiwiZDoyIiwieDowIiwieDoxIiwieDoyIl0sImF0X2hhc2giOiJnQUFBQUFCbjRRZ3VLQWk3Z29xQm5lZ3RUS0hiX1g2RllObHFKclVTLW9lMHQyZXg3Ui1JNjloTFRUWUtkclRSN1hhX2NxWXZ1MzMyaTlNTERpNDRuNVZwYlVWMW43dGMtaUpRdTBzWmVZblJpWlpxMWlBdTRIQT0iLCJkaXNwbGF5X25hbWUiOiIiLCJvbXMiOiJLMSIsImhzbV9rZXkiOiIyMWIyNzc2MDEyNDk1ZmYwMzdlMDY5MTc3ZTQ2ODRkMmZjNTI2ZDNkODZhYjEzYjA3OGExNTc2MyIsImlzRGRwaUVuYWJsZWQiOiJOIiwiaXNNdGZFbmFibGVkIjoiTiIsImZ5X2lkIjoiWVYxNjU2OSIsImFwcFR5cGUiOjEwMCwiZXhwIjoxNzQyODYyNjAwLCJpYXQiOjE3NDI4MDA5NDIsImlzcyI6ImFwaS5meWVycy5pbiIsIm5iZiI6MTc0MjgwMDk0Miwic3ViIjoiYWNjZXNzX3Rva2VuIn0.q6gG-CQMkYXI14uluXuH4M2a31s-P5nZuFrSjV1jpRI"; // Replace with actual token
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
                        stockHistory.from_date = rangform;
                        stockHistory.to_date = rangeto;
                        stockHistory.symbol = sym;
                        stockHistory.Createddate = DateTime.UtcNow;
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

        public void Connect()
        {
            try
            {
                List<string?> symlist = new List<string?>();
                using (var scope = _serviceProvider.CreateScope())
                {
                    var stockssym = scope.ServiceProvider.GetRequiredService<IStockSymbolRepository>();
                    var sym = stockssym.Getallsym();
                    if (sym.Any() && sym != null)
                    {
                        symlist = sym.Where(x => x.Status == true).Select(x => x.Symbol).ToList();
                    }
                }
               
                FyersClass fyersModel = FyersClass.Instance;
                fyersModel.ClientId = ClientId;
                fyersModel.AccessToken = AccessToken;
              
                Methods t = new Methods(this); // Pass the HubContext
                 t.DataWebSocket(symlist);
            }
            catch (Exception ex)
            {
                LogFilegenerate(ex, "FyersWebSocketService");
            }
            
        }

        public void Disconnect()
        {
            _stocklist.Clear();
           
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

        public void DataWebSocket(List<string> stock)
        {
            try
            {
                client = new FyersSocket();
                //client.ReconnectAttemptsCount = 1;
                client.webSocketDelegate = this;

                 client.Connect();
                client.ConnectHSM(ChannelModes.FULL);

                //var stocklist = _service.GetStockList();
                client.SubscribeData(stock);
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
            try
            {
                _service.LogFilegenerate(null, "FyersWebSocketService/Methods" + index);
                Console.WriteLine("Index Data: " + index);
            }
            catch (Exception ex)
            {
                _service.LogFilegenerate(ex, "FyersWebSocketService/Methods" + index);
                Console.WriteLine("Index Data: " + index);
            }
          
        }

        public void OnMessage(JObject response)
        {
            try
            {
                _service.LogFilegenerate(null, "FyersWebSocketService/Methods" + response);
                Console.WriteLine("OnMessage: " + response);
            }
            catch (Exception ex)
            {
                _service.LogFilegenerate(ex, "FyersWebSocketService/Methods" + response);
            }
            
        }

        public void OnOpen(string status)
        {
            try
            {
                _service.LogFilegenerate(null, "FyersWebSocketService/Methods" + status);
                Console.WriteLine("WebSocket Connection Opened: " + status);

            }
            catch (Exception ex)
            {
                _service.LogFilegenerate(ex, "FyersWebSocketService/Methods");
            }
           
        }

        public void OnOrder(JObject orders)
        {
            try
            {
                _service.LogFilegenerate(null, "FyersWebSocketService/Methods" + orders);
                Console.WriteLine("Order Data: " + orders);

            }
            catch (Exception ex)
            {
                _service.LogFilegenerate(ex, "FyersWebSocketService/Methods" + orders);
            }
            
        }

        public void OnPosition(JObject positions)
        {
            try
            {
                _service.LogFilegenerate(null, "FyersWebSocketService/Methods" + positions);
                Console.WriteLine("Position Data: " + positions);
            }
            catch (Exception ex)
            {

                _service.LogFilegenerate(ex, "FyersWebSocketService/Methods" + positions);
            }
           
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
            try
            {
                _service.LogFilegenerate(null, "FyersWebSocketService/Methods" + trades);
                Console.WriteLine("Trade Data: " + trades);
            }
            catch (Exception ex)
            {
                _service.LogFilegenerate(ex, "FyersWebSocketService/Methods" + trades);
            }
            
        }
    }
}
