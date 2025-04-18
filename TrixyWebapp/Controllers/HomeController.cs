using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.FyersWebSocketServices;
using Repository.IRepositories;
using Repository.Models;
using System.Text;
using TrixyWebapp.Helpers;
using TrixyWebapp.ViewModels;


namespace TrixyWebapp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly FyersWebSocketService _fyersWebSocket;
        private readonly IWebStockRepository _stockRepository;
        private readonly IUserRepository _user;
        private readonly IWebHostEnvironment _env;
        private readonly IStockSymbolRepository _stockSymbol;

        public HomeController(FyersWebSocketService fyersWebSocket,
            IRepository<Historical_Data> userRepository,
            IRepository<Strategy> strategyRepository,
            IRepository<User> masterRepository,
            IWebStockRepository stockRepository,
            IUserRepository user,
            IWebHostEnvironment env,
            IStockSymbolRepository stockSymbolRepository)
        {
            _fyersWebSocket = fyersWebSocket;
            _stockRepository = stockRepository;
            _user = user;
            _env = env;
            _stockSymbol = stockSymbolRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                List<StockData> stockData = new List<StockData>();


                var userJson = HttpContext?.Session.GetString("User");
                var userId = HttpContext?.Session.GetString("UserId");
                Helper.LogFile(userJson ?? "", _env);
                Helper.LogFile(userId ?? "", _env);
                ViewData["UserId"] = userId;
                var user = userId != null ? _user.GetById(userId) : new User();


                stockData = _fyersWebSocket.GetStockData(user?.Stocks?.ToList());
                if (user?.Stocks?.Any() == true)
                {
                    foreach (var item in stockData)
                    {
                        item.recommendation = await Recomddation(item?.Symbol ?? "");
                    }
                }
                var user1 = userId != null ? _user.GetById(userId) : new User();
                HttpContext?.Session.SetString("User", System.Text.Json.JsonSerializer.Serialize(user1));
                return View(stockData);
            }
            catch (Exception ex)
            {
                Helper.LogFilegenerate(ex, "Login Action", _env);
                return View();
            }

        }

        [HttpGet]
        public IActionResult RealTimeData()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                //string userId = Encoding.UTF8.GetString(userIdBytes);
                var user = userId != null ? _user.GetById(userId) : new User();
                List<StockData> stockData = _fyersWebSocket.GetStockData(user?.Stocks?.ToList());
                
                var formateddata = stockData.Select(x => new
                {
                    symbol = x.Symbol,
                    change = x.Change,
                    price = x.Price,
                }).ToList();
                return Json(formateddata);
            }
            catch (Exception ex)
            {

                Helper.LogFilegenerate(ex, "Login Action", _env);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> FetchData(string sym)
        {


            var marketStart = new TimeSpan(9, 15, 0);
            var marketEnd = new TimeSpan(15, 30, 0);

            var gethistoricaldata = await _stockRepository.GetStockDataBySymbolAsync(sym);

            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var oneWeekAgo = DateTime.UtcNow.AddDays(-7);
            var oneWeekAgoUtc = DateTime.UtcNow.AddDays(-30);
            var onemonthAgoIST = TimeZoneInfo.ConvertTimeFromUtc(oneWeekAgoUtc, istTimeZone);


            var filteredData = gethistoricaldata
       .Select(item => new
       {
           TimestampIST = TimeZoneInfo.ConvertTimeFromUtc(item.Timestamp, istTimeZone),
           item.Open,
           item.High,
           item.Low,
           item.Close
       })
       .Where(item =>
           item.TimestampIST >= onemonthAgoIST &&
           item.TimestampIST.TimeOfDay >= marketStart &&
           item.TimestampIST.TimeOfDay <= marketEnd)
       .DistinctBy(item => item.TimestampIST) // remove same datetime duplicates
       .OrderBy(item => item.TimestampIST)
       .ToList();
            var groupedByDate = filteredData
      .GroupBy(item => item.TimestampIST.Date)
      .Select(group => new
      {
          Date = group.Key.ToString("yyyy-MM-dd"),
          Data = group.Select(item => new
          {
              x = item.TimestampIST.ToString("yyyy-MM-dd HH:mm"),
              y = new decimal[] { item.Open, item.High, item.Low, item.Close }
          }).ToList()
      });
            var last5data = await _fyersWebSocket.FetchAndHistoricalStockDataAsync(sym, oneWeekAgo.ToString("yyyy-MM-dd"), DateTime.UtcNow.ToString("yyyy-MM-dd"), 60);

            var result5daydata = last5data.GroupBy(item => item.Timestamp.Date).Select(group => new
            {
                Date = group.Key.ToString("yyyy-MM-dd"),
                Data = group.Select(item => new ChartDataPoint
                {
                    x = item.Timestamp.ToString("yyyy-MM-dd HH:mm"),
                    y = new decimal[] { item.Open, item.High, item.Low, item.Close }
                }).ToList()
            }).ToList();
            string jsonResult = System.Text.Json.JsonSerializer.Serialize(groupedByDate);

            return Json(result5daydata);

        }
        [HttpGet]
        public async Task<IActionResult> GetChartDetails(string sym)
        {
            var stocks = await _stockSymbol.GetStockBySymbol(sym);
            if (stocks != null)
            {
                var gethistoricaldata = await _stockRepository.GetStockDataBySymbolAsync(sym);

                var last5data = await _fyersWebSocket.FetchAndHistoricalStockDataAsync(sym, DateTime.UtcNow.ToString("yyyy-MM-dd"), DateTime.UtcNow.ToString("yyyy-MM-dd"), 30);

                StockCandleChartVM returndata = new StockCandleChartVM()
                {
                    Symbol = stocks.Symbol,
                    Companylogo = stocks.CompanyIconUrl,
                    CompanyName = stocks.CompanyName

                };
                return Json(returndata);
            }
            else
            {
                return Json(null);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStrategyStatus(string userId, string strategyName, string status)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(strategyName))
            {
                return BadRequest("Invalid data");
            }

            strategyName = strategyName.Replace(" ", "_");
            bool isChecked;

            if (status == "True")
            {
                isChecked = true;
            }
            else
            {
                isChecked = false;
            }

            await _user.UpdateAsyncStrategy(userId, strategyName, isChecked);

            return Ok(new { message = "Strategy updated successfully." });
        }


        public async Task<IActionResult> EnableDisableStratgey(string sym, bool isEnable)
        {
            string finalsignal = string.Empty;
            string? userId = HttpContext.Session.GetString("UserId");

            var weightedsignal = await _user.GetUserSettings();

            var user = userId != null ? _user.GetById(userId) : new User();
            var userstregy = user?.UserStrategy?.Where(x => x.StretagyEnableDisable == true && x.IsActive == true).ToList();
            var gethistoricaldata = await _stockRepository.GetStockDataBySymbolAsync(sym);
            if (gethistoricaldata != null && gethistoricaldata.Count > 0)
            {

                //combination signal

                if (userId != null && userstregy?.Any() == true)
                {
                    finalsignal = SignalGenerator.GetCombinationsignal(weightedsignal, gethistoricaldata, userstregy);
                    await _user.UpdateAsyncUserStocks(userId, sym, isEnable, finalsignal);
                }

            }

            return RedirectToAction("Index");
        }

        public async Task<Dictionary<string, string>> Recomddation(string sym)
        {
            var userId = HttpContext.Session.GetString("UserId");
            // string userId = Encoding.UTF8.GetString(userIdBytes);
            var weightedsignal = await _user.GetUserSettings();

            var user = userId != null ? _user.GetById(userId) : new Repository.Models.User();
            var userstregy = user?.UserStrategy?.Where(x => x.StretagyEnableDisable == true && x.IsActive == true).ToList();
            var gethistoricaldata = await _stockRepository.GetStockDataBySymbolAsync(sym);

            //combination signal
            if (userId != null && userstregy?.Any() == true)
            {
                var finalsignal = SignalGenerator.GetCombinationsignal(weightedsignal, gethistoricaldata, userstregy);
                await _user.UpdateUserStocks(userId, sym, finalsignal);
            }

            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            if (gethistoricaldata != null && gethistoricaldata.Any())
            {
                if (userstregy?.Any() == true)
                {
                    foreach (var item in userstregy)
                    {
                        switch (item.StretagyName)
                        {
                            case "Bollinger_Bands":
                                #region Bollinger Bands Strategy
                                var BBSresult = SignalGenerator.GenerateBuySellSignalsForBB(gethistoricaldata, weightedsignal?.RSIThresholds?.RsiPeriod ?? 0);
                                keyValuePairs["Bollinger_Bands"] = BBSresult;
                                #endregion

                                break;
                            case "MACD":
                                #region MACD (Moving Average Convergence Divergence) Strategy
                                var MACD = SignalGenerator.CalculateMACD(gethistoricaldata, weightedsignal?.MACD_Settings?.ShortEmaPeriod ?? 0, weightedsignal?.MACD_Settings?.LongEmaPeriod ?? 0, weightedsignal?.MACD_Settings?.SignalPeriod ?? 0);
                                keyValuePairs["MACD"] = MACD;

                                #endregion
                                break;
                            case "Mean_Reversion":
                                #region Mean Reversion Strategy
                                var MRSresult = SignalGenerator.CalculateMeanReversion(gethistoricaldata, weightedsignal?.MeanReversion?.Period ?? 0, weightedsignal?.MeanReversion?.Threshold ?? 0);
                                keyValuePairs["Mean_Reversion"] = MRSresult;

                                #endregion
                                break;
                            case "Moving_Average":
                                #region Moving Average Crossover Strategy
                                var result = SignalGenerator.GenerateSignalsforMovingAverageCrossover(gethistoricaldata, shortTerm: weightedsignal?.MovingAverage?.SMA_Periods ?? 0,
                                    longTerm: weightedsignal?.MovingAverage?.LMA_Periods ?? 0);
                                keyValuePairs["Moving_Average"] = result;
                                #endregion

                                break;
                            case "RSI":
                                #region Relative Strength Index (RSI)
                                var RSIresult = SignalGenerator.genratesignalsforRSI(gethistoricaldata, weightedsignal?.RSIThresholds?.RsiPeriod ?? 0,
                                    weightedsignal?.RSIThresholds?.Overbought ?? 0, weightedsignal?.RSIThresholds?.Oversold ?? 0);
                                keyValuePairs["RSI"] = RSIresult;
                                #endregion

                                break;
                            case "VWAP":
                                #region Volume-Weighted Average Price (VWAP) Strategy
                                var VWAP = SignalGenerator.CalculateVWAP(gethistoricaldata);
                                keyValuePairs["VWAP"] = VWAP;
                                #endregion

                                break;
                        }
                    }
                }

            }

            //keyValuePairs["Combine"] = finalsignal;
            return keyValuePairs;
        }
    }
}
