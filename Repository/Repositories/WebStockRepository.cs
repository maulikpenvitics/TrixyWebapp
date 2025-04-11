using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Repository.DbModels;
using Repository.FyersWebSocketServices;
using Repository.IRepositories;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class WebStockRepository : IWebStockRepository
    {
        private readonly IRepository<Historical_Data> _collection;
        private readonly IRepository<Stock_master_data> _stcock;
        private readonly IMongoCollection<Historical_Data> _historicaldata;
        private readonly IMongoCollection<AdminSettings> _adminsetting;
        public WebStockRepository(IRepository<Historical_Data> collection,
            IRepository<Stock_master_data> stcock, IMongoClient mongoClient,
            IOptions<MongoDBSettings> settings)
        {
            _collection = collection;
            _stcock = stcock;
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _historicaldata = database.GetCollection<Historical_Data>("Historical_Data");
            _adminsetting = database.GetCollection<AdminSettings>("AdminSettings");
        }
        public async Task<List<Historical_Data>> GetHistoricalData()
        {
            List<Historical_Data> historical_Datas = new List<Historical_Data>();
            historical_Datas = (List<Historical_Data>)await _collection.GetAllAsync();
            return historical_Datas;
        }
        public async Task InsertStockAsync(Stock_master_data stock)
        {
            List<Stock_master_data> stock_Master_sq=new List<Stock_master_data> ();
            var stock1 = new Stock_master_data
            {
                StockSymbl = "NSE:SBIN-EQ",
                CompanyName = "Apple Inc.",
                Code = "NSE",
                Status = true,
                CreatedBy = "Admin",
                Updatedby = "Admin"
            };
            stock_Master_sq.Add(stock1);
            var stock2 = new Stock_master_data
            {
                StockSymbl = "SBIN-EQ",
                CompanyName = "Reliance",
                Code = "NSE",
                Status = true,
                CreatedBy = "Admin",
                Updatedby = "Admin"
            };
            stock_Master_sq.Add(stock2);
            var stock3 = new Stock_master_data
            {
                StockSymbl = "NSE:SBIN-EQ",
                CompanyName = "ITC",
                Code = "BSE",
                Status = true,
                CreatedBy = "Admin",
                Updatedby = "Admin"
            };
            stock_Master_sq.Add(stock3);
            var stock4 = new Stock_master_data
            {
                StockSymbl = "NSE:SBIN-EQ",
                CompanyName = "TCS",
                Code = "BSE",
                Status = true,
                CreatedBy = "Admin",
                Updatedby = "Admin"
            };
            stock_Master_sq.Add(stock4);
            var stock5 = new Stock_master_data
            {
                StockSymbl = "SBIN-EQ",
                CompanyName = "IDFC First bank.",
                Code = "BSE",
                Status = true,
                CreatedBy = "Admin",
                Updatedby = "Admin"
            };
            stock_Master_sq.Add(stock5);
            var stock6 = new Stock_master_data
            {
                StockSymbl = "SBIN-EQ",
                CompanyName = "Bajaj Finance.",
                Code = "BSE",
                Status = true,
                CreatedBy = "Admin",
                Updatedby = "Admin"
            };
            stock_Master_sq.Add(stock6);
            await _stcock.InsertManyAsync(stock_Master_sq);
           
        }

        public async Task<List<Historical_Data>> GetStockDataBySymbolAsync(string symbol)
        {
            var filter = Builders<Historical_Data>.Filter.And(
                Builders<Historical_Data>.Filter.Eq(s => s.symbol, symbol)
            );

            return await _collection.FindAsync(filter);
        }

        public async Task<int> DeleteHistoricaldata()
        {
            
            var frequency = await _adminsetting
            .Find(_ => true)
           .FirstOrDefaultAsync();
            
            DateTime startOfDay = DateTime.UtcNow.AddDays(-1).Date;  
            DateTime endOfDay = startOfDay.AddDays(1).AddTicks(-1); 
            DateTime HoursAgo = DateTime.UtcNow.AddDays(-1).Date;
            var filter = Builders<Historical_Data>.Filter.And(
     Builders<Historical_Data>.Filter.Gte(s => s.Timestamp, startOfDay),
     Builders<Historical_Data>.Filter.Lte(s => s.Timestamp, endOfDay)
 );
            var result = await _historicaldata.DeleteManyAsync(filter);

            return (int)result.DeletedCount;
        }
        public async Task InsertNewHistoricalData(List<Historical_Data> newData)
        {
            if (newData !=null && newData.Any()) 
            {
                await _historicaldata.InsertManyAsync(newData);
            }
        
        }

    }
}
