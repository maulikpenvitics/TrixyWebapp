using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Repository.DbModels;
using Repository.IRepositories;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class StockSymbolRepository : IStockSymbolRepository
    {
        private readonly IMongoCollection<StockSymbol> _stockSymbol;
        private readonly IErrorHandlingRepository _errorHandlingRepository;
        public StockSymbolRepository(IMongoClient mongoClient, IOptions<MongoDBSettings> settings, IErrorHandlingRepository errorHandlingRepository) 
        {
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _stockSymbol = database.GetCollection<StockSymbol>("StockSymbol");
            _errorHandlingRepository = errorHandlingRepository;
        }

        public List<StockSymbol> Getallsym()
        {
            try
            {
                return _stockSymbol.Find(Builders<StockSymbol>.Filter.And(
                Builders<StockSymbol>.Filter.Eq("Status", true)
            )).ToList();
            }
            catch (Exception ex)
            {
                 _errorHandlingRepository.AddError(ex, "StockSymbolRepository/Getallsym");
                throw new Exception("An error occurred while retrieving data. Please try again.");
            }
           
        }

        public async Task<StockSymbol> GetStockBySymbol(string Symbol)
        {
            try
            {
                return await _stockSymbol.Find(Builders<StockSymbol>.Filter.And(
               Builders<StockSymbol>.Filter.Eq("Symbol", Symbol),
               Builders<StockSymbol>.Filter.Eq("Status", true)
           )).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
               await _errorHandlingRepository.AddErrorHandling(ex, "StockSymbolRepository/GetStockBySymbol");
                throw new Exception("An error occurred while retrieving data. Please try again.");
            }
           
        }

        public async Task<List<StockSymbol>> GetStocklistBySymbol(string Symbol)
        {
            try
            {
                var filter = Builders<StockSymbol>.Filter.And(
                Builders<StockSymbol>.Filter.Or(
                Builders<StockSymbol>.Filter.Regex("Symbol", new BsonRegularExpression(Symbol, "i")) // Case-insensitive search
                ),
             Builders<StockSymbol>.Filter.Eq("Status", true) // Only active stocks
          );

                return await _stockSymbol.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "StockSymbolRepository/GetStocklistBySymbol");
                throw new Exception("An error occurred while retrieving data. Please try again.");
            }
        }
    }
}
