using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    public class ErrorHandlingRepository :IErrorHandlingRepository
    {
        private readonly IMongoCollection<ErrorHandling> _errorhandling;
        private readonly ILogger<ErrorHandlingRepository> _logger;

        public ErrorHandlingRepository(IMongoClient mongoClient, IOptions<MongoDBSettings> settings, ILogger<ErrorHandlingRepository> logger) 
        {
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _errorhandling = database.GetCollection<ErrorHandling>("ErrorHandling");
            _logger = logger;
        }

        public async Task AddErrorHandling(Exception ex,string remarks)
        {
            try
            {
                var errorhandlingobj = new ErrorHandling
                {
                    Exception = ex.ToString(),
                    ExceptionLogTime = DateTime.Now,
                    ExceptionStackTrace = ex.StackTrace,
                    Source = ex.Source,
                    Remarks = remarks
                };
                await _errorhandling.InsertOneAsync(errorhandlingobj);
            }
            catch (Exception innerex)
            {
                _logger.LogError($"MongoDB Error - LogErrorAsync: {innerex.Message}");
            }
        }
        public async Task<IEnumerable<ErrorHandling>> GetAllErrorsAsync()
        {
            return await _errorhandling.Find(_ => true).ToListAsync();
        }
        public  void AddError(Exception ex, string remarks)
        {
            try
            {
                var errorhandlingobj = new ErrorHandling
                {
                    Exception = ex.ToString(),
                    ExceptionLogTime = DateTime.Now,
                    ExceptionStackTrace = ex.StackTrace,
                    Source = ex.Source,
                    Remarks = remarks
                };
                 _errorhandling.InsertOneAsync(errorhandlingobj);
            }
            catch (Exception innerex)
            {
                _logger.LogError($"MongoDB Error - LogErrorAsync: {innerex.Message}");
            }
        }
    }
}
