using FyersCSharpSDK;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.FyersWebSocketServices;
using Repository.IRepositories;
using Repository.Models;
using System.Text;


namespace TrixyWebapp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly FyersWebSocketService _fyersWebSocket;
        private readonly IRepository<Historical_Data> _HistoricalStockdata;
        private readonly IRepository<Strategy> _strategyRepository;
        private readonly IRepository<Master_Strategy_User> _masterRepository;

        public HomeController(FyersWebSocketService fyersWebSocket, IRepository<Historical_Data> userRepository, IRepository<Strategy> strategyRepository, IRepository<Master_Strategy_User> masterRepository)
        {
            _fyersWebSocket = fyersWebSocket;
            _HistoricalStockdata = userRepository;
            _strategyRepository = strategyRepository;
            _masterRepository = masterRepository;
        }
        
        public async Task<IActionResult> Index()
        {
            var userIdBytes = HttpContext.Session.Get("UserId");
            string userId = Encoding.UTF8.GetString(userIdBytes);
            ViewData["UserId"] = userId;

            var UserRole = HttpContext.Session.Get("UserRole");
            string userRole = Encoding.UTF8.GetString(UserRole);
            ViewData["UserRole"] = userRole;

            var data = await _fyersWebSocket.FetchAndStoreHistoricalStockDataAsync();
            //await _HistoricalStockdata.InsertManyAsync(data);
            var gethistoricaldata = await _HistoricalStockdata.GetAllAsync();

            var masterData = await _masterRepository.GetByIdAsyncForMaster(userId);
            ViewData["MasterData"] = masterData;

            return View();
        }

        [HttpGet]
        public IActionResult RealTimeData()
        {
            List<StockData> stockData = _fyersWebSocket.GetStockData();

            var formattedData = stockData.Select(s => new {
                symbol = s.Symbol,
                price = s.Price,
                change = s.Change
            });

            return Json(formattedData);
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

            await _masterRepository.UpdateAsyncStrategy(userId, strategyName, isChecked);

            return Ok(new { message = "Strategy updated successfully." });
        }

    }
}
