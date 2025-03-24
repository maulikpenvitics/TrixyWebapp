using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Repository.FyersWebSocketServices;
using Repository.IRepositories;
using Repository.Models;
using Repository.Repositories;
using System.Text;

namespace TrixyWebapp.Controllers
{
    public class StockSymbolController : Controller
    {
        private readonly IRepository<StockSymbol> _stockSymbolRepository;
        private readonly IStockSymbolRepository _stockSymbol;
        private readonly IUserRepository _userRepository;
        private readonly IRepository<User> _masterRepository;
        private readonly FyersWebSocketService _fyersWebSocket;
        public StockSymbolController(IRepository<StockSymbol> stockSymbolRepository, 
            IStockSymbolRepository stockSymbol,
            IUserRepository userRepository,
            IRepository<User> masterRepository,
            FyersWebSocketService fyersWebSocketService)
        {
            _stockSymbolRepository = stockSymbolRepository;
            _stockSymbol = stockSymbol;
            _userRepository = userRepository;
            _masterRepository = masterRepository;
            _fyersWebSocket = fyersWebSocketService;
        }
        public async Task<IActionResult> Index()
        {
            var symbollist = await _stockSymbolRepository.GetAllAsync();
            return View(symbollist);

        }


        [HttpGet]
        public async Task<IActionResult> CreateSymbol(string? Id)
        {
            if (!string.IsNullOrEmpty(Id))
            {
                var stockSymbol = await _stockSymbolRepository.GetByIdAsync(Id);
                if (stockSymbol != null)
                {
                    // Get the base URL dynamically
                    var origin = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

                    // Append the relative file path to get the full URL
                    stockSymbol.CompanyIconUrl = $"{origin}{stockSymbol.CompanyIconUrl}";

                    return View(stockSymbol); // Pass existing user data for editing
                }
            }
            return View(new StockSymbol());
        }

        [HttpPost]
        public async Task<IActionResult> CreateSymbol(StockSymbol stockSymbol)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload
                if (stockSymbol.IconFile != null && stockSymbol.IconFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(stockSymbol.IconFile.FileName)}";
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await stockSymbol.IconFile.CopyToAsync(stream);
                    }

                    // Store only the relative path in the database
                    stockSymbol.CompanyIconUrl = "/Uploads/" + fileName;
                }

                // **EDIT MODE: If ID exists, update the existing record**
                if (stockSymbol.Id.ToString() != null && stockSymbol.Id != ObjectId.Empty && stockSymbol.Id.ToString() != "000000000000000000000000")
                {
                   // await _userRepository.UpdateAsyncUserStocksLogo(stockSymbol.Symbol, stockSymbol.CompanyIconUrl);
                    var existingStock = await _stockSymbolRepository.GetByIdAsync(stockSymbol.Id.ToString());

                    if (existingStock != null)
                    {
                        // If no new file is uploaded, retain the old file path
                        if (string.IsNullOrEmpty(stockSymbol.CompanyIconUrl))
                        {
                            stockSymbol.CompanyIconUrl = existingStock.CompanyIconUrl;
                        }

                        await _stockSymbolRepository.UpdateAsync(stockSymbol.Id.ToString(), stockSymbol);
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    // **CREATE MODE: Insert new record**
                    var existuser = await _stockSymbol.GetStockBySymbol(stockSymbol?.Symbol ?? "");
                    if (existuser == null)
                    {
                        var result = await _stockSymbolRepository.InsertAsync(stockSymbol);
                        if (result == 1)
                        {
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            ViewBag.Errormessage = "Please try again";
                            return View();
                        }
                    }
                    else
                    {
                        ViewBag.Errormessage = "This user already exists.";
                        return View();
                    }
                }
            }

            ViewBag.Errormessage = "Please try again";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetStockList(string stocksym)
        {
            if (!string.IsNullOrEmpty(stocksym))
            {
                var stocklist = await _stockSymbol.GetStocklistBySymbol(stocksym);
                var stocklst = stocklist.Select(x => new
                {
                    symbol = x.Symbol,
                    id = x.Id.ToString(),
                });
                return Json(stocklst);
            }
            else
            {
                return Json(new List<Stocks>());
            }
           
        }

        [HttpPost]
        public async Task<IActionResult> AddUserStock(string Symbol,string StockId)
        {
            var userIdBytes = HttpContext.Session.Get("UserId");
            string userId = Encoding.UTF8.GetString(userIdBytes);
            var user = await _masterRepository.GetByIdAsync(userId);
            if (!string.IsNullOrEmpty(StockId))
            {
                var stockSymbol = await _stockSymbolRepository.GetByIdAsync(StockId);
                if (stockSymbol!=null)
                {
                    if (user != null)
                    {
                        if (user.Stocks == null)
                        {
                            user.Stocks.Add(new Stocks
                            {
                                CompanyLogoUrl = stockSymbol.CompanyIconUrl,
                                CompanyName = stockSymbol.CompanyName,
                                IsActive = true,
                                StockNotification = false,
                                BuySellSignal = null,
                                Symbol = stockSymbol.Symbol,
                            });
                        }
                        else
                        {
                            if (user.Stocks.Where(x=>x.Symbol==stockSymbol.Symbol).FirstOrDefault()==null)
                            {
                                user.Stocks.Add(new Stocks
                                {
                                    CompanyLogoUrl = stockSymbol.CompanyIconUrl,
                                    CompanyName = stockSymbol.CompanyName,
                                    IsActive = true,
                                    StockNotification = false,
                                    BuySellSignal = null,
                                    Symbol = stockSymbol.Symbol,
                                });
                            }
                        }
                       var updateuser= await _userRepository.AddUserStocks(user);
                        if (updateuser)
                        {
                           // _fyersWebSocket.Connect(user.Stocks.ToList());
                            ViewBag.succesmessage = "Added stock succesfully";
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
            }
            return RedirectToAction("Index", "Home");

        }
       
        [HttpGet]
        public async Task<IActionResult> DeleteSymbol(string Id)
        {
            var symbol = await _stockSymbolRepository.GetByIdAsync(Id);
            if (symbol != null)
            {
                symbol.Status = false;
                await _stockSymbolRepository.UpdateAsync(symbol.Id.ToString(), symbol);
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Index");
            }

        }
    }
}
