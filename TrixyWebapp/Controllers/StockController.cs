using Microsoft.AspNetCore.Mvc;
using Repository.IRepositories;
using Repository.Models;

namespace TrixyWebapp.Controllers
{
    public class StockController : Controller
    {
       // private readonly IStockRepository _stockRepository;

        //public StockController(IStockRepository stockRepository)
        //{
        //    _stockRepository = stockRepository;
        //}

        public IActionResult Index()
        {
            var stock = new Stock_master_data
            {
                StockSymbl = "AAPL",
                CompanyName = "Apple Inc.",
                Code = "NSE",
                Status = true,
                CreatedBy = "Admin",
                Updatedby = "Admin"
            };

           // await _stockRepository.InsertStockAsync(stock);
            return View();
        }
    }
}
