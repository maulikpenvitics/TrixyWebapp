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
        private readonly IErrorHandlingRepository _errorHandlingRepository;
        public WebStockRepository(IRepository<Historical_Data> collection,
            IRepository<Stock_master_data> stcock, IMongoClient mongoClient,
            IOptions<MongoDBSettings> settings, IErrorHandlingRepository errorHandlingRepository)
        {
            _collection = collection;
            _stcock = stcock;
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _historicaldata = database.GetCollection<Historical_Data>("Historical_Data");
            _adminsetting = database.GetCollection<AdminSettings>("AdminSettings");
            _errorHandlingRepository = errorHandlingRepository;
        }
        public async Task<List<Historical_Data>> GetHistoricalData()
        {
            try
            {
                List<Historical_Data> historical_Datas = new List<Historical_Data>();
                historical_Datas = (List<Historical_Data>)await _collection.GetAllAsync();
                return historical_Datas;
            }
            catch (Exception ex)
            {

                await _errorHandlingRepository.AddErrorHandling(ex, "WebStockRepository/GetHistoricalData");
                throw new Exception("An error occurred while retrieving data. Please try again.");
            }
          
        }

        public async Task<List<Historical_Data>> GetStockDataBySymbolAsync(string symbol)
        {
            try
            {
                var filter = Builders<Historical_Data>.Filter.And(
               Builders<Historical_Data>.Filter.Eq(s => s.symbol, symbol)
           );

                return await _collection.FindAsync(filter);
            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "WebStockRepository/GetStockDataBySymbolAsync");
                throw new Exception("An error occurred while retrieving data. Please try again.");
            }
           
        }

        public async Task<int> DeleteHistoricaldata()
        {
            try
            {
                DateTime cutoffDate = DateTime.UtcNow.AddDays(-90);

                // Define a filter to delete documents older than 90 days
                var filter = Builders<Historical_Data>.Filter.Lt(s => s.Timestamp, cutoffDate);

                // Delete the matching documents
                var result = await _historicaldata.DeleteManyAsync(filter);
                var dayliydeletedata = everydayDeleteHistoricaldata();
                return (int)result.DeletedCount;

                
            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "WebStockRepository/DeleteHistoricaldata");
                throw new Exception("An error occurred while DeleteHistorical data. Please try again.");
            }
            
           
        }

        public async Task<int> everydayDeleteHistoricaldata()
        {
            try
            {
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
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "WebStockRepository/DeleteHistoricaldata");
                throw new Exception("An error occurred while DeleteHistorical data. Please try again.");
            }


        }
        public async Task InsertNewHistoricalData(List<Historical_Data> newData)
        {
            try
            {
                if (newData != null && newData.Any())
                {
                    await _historicaldata.InsertManyAsync(newData);
                }

            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "WebStockRepository/InsertNewHistoricalData");
                throw new Exception("An error occurred while InsertNewHistoricalData data. Please try again.");
            }
           
        
        }

    }
}
