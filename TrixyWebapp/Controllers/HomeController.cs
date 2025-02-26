using FyersCSharpSDK;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.FyersWebSocketServices;
using Repository.IRepositories;
using Repository.Models;


namespace TrixyWebapp.Controllers
{
   
    public class HomeController : Controller
    {
        private readonly FyersWebSocketService _fyersWebSocket;
        private readonly IRepository<Historical_Data> _HistoricalStockdata;
        
        public HomeController(FyersWebSocketService fyersWebSocket, IRepository<Historical_Data> userRepository)
        {
            _fyersWebSocket = fyersWebSocket;
            _HistoricalStockdata = userRepository;
        }
        
        public async Task<IActionResult> Index()
        {
           // var data = await _fyersWebSocket.FetchAndStoreHistoricalStockDataAsync();
            //await _HistoricalStockdata.InsertManyAsync(data);
            var gethistoricaldata = await _HistoricalStockdata.GetAllAsync();
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
    }
}
