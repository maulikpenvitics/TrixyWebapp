using FyersCSharpSDK;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Core.Types;
using Repository.FyersWebSocketServices;
using Repository.IRepositories;
using Repository.Models;
using Repository.Repositories;
using System.Text;
using TrixyWebapp.Helpers;
using TrixyWebapp.ViewModels;


namespace TrixyWebapp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly FyersWebSocketService _fyersWebSocket;
        private readonly IRepository<Historical_Data> _HistoricalStockdata;
        private readonly IRepository<Strategy> _strategyRepository;
        private readonly IRepository<User> _masterRepository;
        private readonly IWebStockRepository _stockRepository;
        private readonly IUserRepository _user;
        public HomeController(FyersWebSocketService fyersWebSocket,
            IRepository<Historical_Data> userRepository, 
            IRepository<Strategy> strategyRepository,
            IRepository<User> masterRepository,
            IWebStockRepository stockRepository,
            IUserRepository user)
        {
            _fyersWebSocket = fyersWebSocket;
            _HistoricalStockdata = userRepository;
            _strategyRepository = strategyRepository;
            _masterRepository = masterRepository;
            _stockRepository = stockRepository;
            _user = user;
        }
        
        public async Task<IActionResult> Index()
        {
           
            var userIdBytes = HttpContext.Session.Get("UserId");
            string userId = Encoding.UTF8.GetString(userIdBytes);
            ViewData["UserId"] = userId;

            var UserRole = HttpContext.Session.Get("UserRole");
            string userRole = Encoding.UTF8.GetString(UserRole);
            ViewData["UserRole"] = userRole;
            var masterData = await _masterRepository.GetByIdAsyncForMaster(userId);
            ViewData["MasterData"] = masterData;
            //var data = await _fyersWebSocket.FetchAndStoreHistoricalStockDataAsync();

            //await _HistoricalStockdata.InsertManyAsync(data);
            var gethistoricaldata = await _HistoricalStockdata.GetAllAsync();
            //var gethistoricaldata = await _stockRepository.GetStockDataBySymbolAsync("NSE:OFSS - EQ");
           // var weightedsignal = await _user.GetUserSettings(userId);

            //EnableDisableStratgey("NSE:OFSS-EQ");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> RealTimeData()
        {
            List<StockData> stockData = _fyersWebSocket.GetStockData();
            return PartialView("_RealStockData", stockData);
        }

        [HttpGet]
        public async Task<IActionResult> FetchData()
        {
            var marketStart = new TimeSpan(9, 15, 0);
            var marketEnd = new TimeSpan(15, 30, 0);

            var gethistoricaldata = await _HistoricalStockdata.GetAllAsync();

            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            var formattedData = gethistoricaldata
                .Select(item => new
                {
                    TimestampIST = TimeZoneInfo.ConvertTimeFromUtc(item.Timestamp, istTimeZone),
                    item.Open,
                    item.High,
                    item.Low,
                    item.Close
                })
                .Where(item => item.TimestampIST.TimeOfDay >= marketStart && item.TimestampIST.TimeOfDay <= marketEnd)
                .OrderBy(item => item.TimestampIST)
                .Select(item => new
                {
                    x = item.TimestampIST.ToString("yyyy-MM-dd HH:mm"),  // Keep trading time only
                    y = new decimal[] { item.Open, item.High, item.Low, item.Close } // OHLC
                });

            return Json(formattedData);

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

            if(status == "True")
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

       
        public async Task<IActionResult> EnableDisableStratgey(string sym,bool isEnable)
        {
            string finalsignal=string.Empty;
            var userIdBytes = HttpContext.Session.Get("UserId");
            string userId = Encoding.UTF8.GetString(userIdBytes);
            var weightedsignal = await _user.GetUserSettings(userId);
            await _user.UpdateAsyncUserStocks(userId,sym, isEnable);
            var gethistoricaldata = await _stockRepository.GetStockDataBySymbolAsync(sym);
            if (gethistoricaldata!=null && gethistoricaldata.Count>0)
            {
                #region Moving Average Crossover Strategy
               

                var result = SignalGenerator.GenerateSignalsforMovingAverageCrossover(gethistoricaldata, shortTerm: 10, longTerm: 50);

                #endregion

                #region Relative Strength Index (RSI)
                var RSIresult = SignalGenerator.genratesignalsforRSI(gethistoricaldata);

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
                var MACD = SignalGenerator.CalculateMACD(gethistoricaldata, 12, 26, 9);
                #endregion
                //combination signal
                finalsignal = SignalGenerator.GetCombinationsignal(weightedsignal, gethistoricaldata);
            }

            return Json(new {finalsignal= finalsignal } );
        }

    }
}
