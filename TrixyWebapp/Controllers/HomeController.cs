using FyersCSharpSDK;
using Microsoft.AspNetCore.Mvc;
using Repository.FyersWebSocketServices;


namespace TrixyWebapp.Controllers
{
    public class HomeController : Controller
    {
        private readonly FyersWebSocketService _fyersWebSocket;

        public HomeController(FyersWebSocketService fyersWebSocket)
        {
            _fyersWebSocket = fyersWebSocket;
        }

        public IActionResult Index()
        {
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
