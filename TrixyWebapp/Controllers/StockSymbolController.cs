using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Repository.IRepositories;
using Repository.Models;
using Repository.Repositories;

namespace TrixyWebapp.Controllers
{
    public class StockSymbolController : Controller
    {
        private readonly IRepository<StockSymbol> _stockSymbolRepository;
        private readonly IStockSymbolRepository _stockSymbol;

        public StockSymbolController(IRepository<StockSymbol> stockSymbolRepository, IStockSymbolRepository stockSymbol)
        {
            _stockSymbolRepository = stockSymbolRepository;
            _stockSymbol = stockSymbol;
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
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "/wwwroot/uploads");
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
                    stockSymbol.CompanyIconUrl = "/uploads/" + fileName;
                }

                // **EDIT MODE: If ID exists, update the existing record**
                if (stockSymbol.Id.ToString() != null && stockSymbol.Id != ObjectId.Empty && stockSymbol.Id.ToString() != "000000000000000000000000")
                {
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


        //[HttpPost]
        //public async Task<IActionResult> CreateSymbol(StockSymbol stockSymbol)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if (stockSymbol.IconFile != null && stockSymbol.IconFile.Length > 0)
        //        {

        //        }


        //        if (stockSymbol.Id.ToString() != null && stockSymbol.Id != ObjectId.Empty && stockSymbol.Id.ToString() != "000000000000000000000000")
        //        {
        //            await _stockSymbolRepository.UpdateAsync(stockSymbol.Id.ToString(), stockSymbol);
        //            return RedirectToAction("Index");
        //        }
        //        else
        //        {
        //            var existuser = await _stockSymbol.GetStockBySymbol(stockSymbol?.Symbol ?? "");
        //            if (existuser == null)
        //            {
        //                var result = await _stockSymbolRepository.InsertAsync(stockSymbol);
        //                if (result == 1)
        //                {
        //                    return RedirectToAction("Index");
        //                }
        //                else
        //                {
        //                    ViewBag.Errormessage = "Please try agin";
        //                    return View();
        //                }
        //            }
        //            else
        //            {
        //                ViewBag.Errormessage = "This user already exists.";
        //                return View();
        //            }
        //        }

        //    }
        //    else
        //    {
        //        ViewBag.Errormessage = "Please try agin";
        //        return View();
        //    }
        //}

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
