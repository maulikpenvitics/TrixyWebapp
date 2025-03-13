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
    public class StockSymbolRepository : MongoRepository<User>, IStockSymbolRepository
    {
        private readonly IMongoCollection<StockSymbol> _stockSymbol;
        public StockSymbolRepository(IMongoClient mongoClient, IOptions<MongoDBSettings> settings) : base(mongoClient, settings)
        {
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _stockSymbol = database.GetCollection<StockSymbol>("StockSymbol");
        }

        public async Task<StockSymbol> GetStockBySymbol(string Symbol)
        {
            return await _stockSymbol.Find(Builders<StockSymbol>.Filter.And(
                Builders<StockSymbol>.Filter.Eq("Symbol", Symbol),
                Builders<StockSymbol>.Filter.Eq("Status", true)
            )).FirstOrDefaultAsync();
        }

        public async Task<List<StockSymbol>> GetStocklistBySymbol(string Symbol)
        {
            var filter = Builders<StockSymbol>.Filter.And(
                      Builders<StockSymbol>.Filter.Or(
                      Builders<StockSymbol>.Filter.Regex("Symbol", new BsonRegularExpression(Symbol, "i")) // Case-insensitive search
                      ),
                   Builders<StockSymbol>.Filter.Eq("Status", true) // Only active stocks
                );

            return await _stockSymbol.Find(filter).ToListAsync();

        }
    }
}
