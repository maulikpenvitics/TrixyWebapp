using FyersCSharpSDK;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Model;
using Microsoft.VisualBasic;
using NuGet.Protocol.Core.Types;
using Repository.FyersWebSocketServices;
using Repository.IRepositories;
using Repository.Models;
using Repository.Repositories;
using System.Text;
using System.Text.Json;
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

        public IActionResult Index()
        {
            try
            {
                var userIdBytes = HttpContext.Session.Get("UserId");
                string userId = Encoding.UTF8.GetString(userIdBytes);
                ViewData["UserId"] = userId;

                var UserRole = HttpContext.Session.Get("UserRole");
                string userRole = Encoding.UTF8.GetString(UserRole);
                ViewData["UserRole"] = userRole;

                var user = _user.GetById(userId);
                HttpContext.Session.SetString("User", JsonSerializer.Serialize(user));

                List<StockData> stockData = _fyersWebSocket.GetStockData(user?.Stocks?.ToList());
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
                var userIdBytes = HttpContext.Session.Get("UserId");
                string userId = Encoding.UTF8.GetString(userIdBytes);
                var user = _user.GetById(userId);
                List<StockData> stockData = _fyersWebSocket.GetStockData(user?.Stocks?.ToList());
                var formateddata = stockData.Select(x => new
                {
                    symbol = x.Symbol,
                    change = x.Change,
                    price = x.Price,
                }).ToList();
                return Json(formateddata);
              //  return PartialView("_RealStockData", stockData);
            }
            catch (Exception ex)
            {

                Helper.LogFilegenerate(ex, "Login Action", _env);
            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Stocksdata()
        {
            var userIdBytes = HttpContext.Session.Get("UserId");
            string userId = Encoding.UTF8.GetString(userIdBytes);
            var user = _user.GetById(userId);
            List<StockData> stockData = _fyersWebSocket.GetStockData(user?.Stocks?.ToList());
            return PartialView("_RealStockData", stockData);
        }

        [HttpGet]
        public async Task<IActionResult> FetchData(string sym)
        {
            var marketStart = new TimeSpan(9, 15, 0);
            var marketEnd = new TimeSpan(15, 30, 0);

            var gethistoricaldata = await _stockRepository.GetStockDataBySymbolAsync(sym);

            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var oneWeekAgo = DateTime.UtcNow.AddDays(-30);
            var oneWeekAgoUtc = DateTime.UtcNow.AddDays(-30);
            var oneWeekAgoIST = TimeZoneInfo.ConvertTimeFromUtc(oneWeekAgoUtc, istTimeZone);
            var formattedData = gethistoricaldata
                .Select(item => new
                {
                    TimestampIST = TimeZoneInfo.ConvertTimeFromUtc(item.Timestamp, istTimeZone),
                    item.Open,
                    item.High,
                    item.Low,
                    item.Close
                })
                .Where(item => item.TimestampIST >= oneWeekAgoIST && item.TimestampIST.TimeOfDay >= marketStart && item.TimestampIST.TimeOfDay <= marketEnd)
                .OrderBy(item => item.TimestampIST)
                .Select(item => new
                {
                    x = item.TimestampIST.ToString("yyyy-MM-dd HH:mm"),  // Keep trading time only
                    y = new decimal[] { item.Open, item.High, item.Low, item.Close } // OHLC
                });

            return Json(formattedData);

        }
        [HttpGet]
        public async Task<IActionResult> GetChartDetails(string sym)
        {
            var stocks = await _stockSymbol.GetStockBySymbol(sym);
            if (stocks != null)
            {
                var gethistoricaldata = await _stockRepository.GetStockDataBySymbolAsync(sym);
                var historicaldata = gethistoricaldata.OrderByDescending(x => x.Timestamp).FirstOrDefault();
                StockCandleChartVM returndata = new StockCandleChartVM()
                {
                    Symbol = stocks.Symbol,
                    Companylogo = stocks.CompanyIconUrl,
                    CompanyName = stocks.CompanyName,
                    Currentprice = historicaldata?.Open,
                    High = historicaldata?.High,
                    low = historicaldata?.Low,
                    close = historicaldata?.Close,
                    volume = historicaldata?.Volume
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
            var userIdBytes = HttpContext.Session.Get("UserId");
            string userId = Encoding.UTF8.GetString(userIdBytes);
            var weightedsignal = await _user.GetUserSettings();

            var gethistoricaldata = await _stockRepository.GetStockDataBySymbolAsync(sym);
            if (gethistoricaldata != null && gethistoricaldata.Count > 0)
            {
                #region Moving Average Crossover Strategy


                var result = SignalGenerator.GenerateSignalsforMovingAverageCrossover(gethistoricaldata, shortTerm: weightedsignal.MACD_Settings.ShortEmaPeriod, 
                    longTerm: weightedsignal.MACD_Settings.LongEmaPeriod);

                #endregion

                #region Relative Strength Index (RSI)
                var RSIresult = SignalGenerator.genratesignalsforRSI(gethistoricaldata, weightedsignal.RSIThresholds.RsiPeriod,
                    weightedsignal.RSIThresholds.Overbought, weightedsignal.RSIThresholds.Oversold);

                #endregion
                #region Bollinger Bands Strategy
                var BBSresult = SignalGenerator.GenerateBuySellSignalsForBB(gethistoricaldata);
                #endregion

                #region Mean Reversion Strategy
                var MRSresult = SignalGenerator.CalculateMeanReversion(gethistoricaldata, 30, 0.1);
                #endregion

                #region Volume-Weighted Average Price (VWAP) Strategy
                var VWAP = SignalGenerator.CalculateVWAP(gethistoricaldata);
                #endregion

                #region MACD (Moving Average Convergence Divergence) Strategy
                var MACD = SignalGenerator.CalculateMACD(gethistoricaldata, weightedsignal.MACD_Settings.ShortEmaPeriod, weightedsignal.MACD_Settings.LongEmaPeriod, weightedsignal.MACD_Settings.SignalPeriod);
                #endregion
                //combination signal
                finalsignal = SignalGenerator.GetCombinationsignal(weightedsignal, gethistoricaldata);
                await _user.UpdateAsyncUserStocks(userId, sym, isEnable, finalsignal);
            }

            return RedirectToAction("Index");
        }

    }
}
