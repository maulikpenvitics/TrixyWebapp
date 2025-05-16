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
    public  class AdminSettingRepository : IAdminSettingRepository
    {
        private readonly IMongoCollection<AdminSettings> _adminSettings;
        private readonly IErrorHandlingRepository _errorHandlingRepository;
        public AdminSettingRepository(IMongoClient mongoClient, IOptions<MongoDBSettings> settings, IErrorHandlingRepository errorHandlingRepository) 
        {
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _adminSettings = database.GetCollection<AdminSettings>("AdminSettings");
            _errorHandlingRepository = errorHandlingRepository;
        }
        public async Task<string> GetJobFrequencyAsync()
        {
            try
            {
                var setting = await _adminSettings.Find(_ => true).FirstOrDefaultAsync();
                return setting.Frequency > 0 ? setting.Frequency.ToString() : "1";
            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "AdminSettingRepository/GetJobFrequencyAsync");
                return "1";

            }
           
        }
        public async Task<bool> UpdateUserAuthcode(string authcode)
        {
            try
            {
                var existingSetting = await _adminSettings.Find(_ => true).FirstOrDefaultAsync();

                if (existingSetting == null)
                    return false; // No document to update

                var filter = Builders<AdminSettings>.Filter.Eq(x => x.Id, existingSetting.Id);
                var update = Builders<AdminSettings>.Update.Set(x => x.UserAuthtoken.code, authcode);
                    
                var result = await _adminSettings.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "AdminSettingRepository/GetJobFrequencyAsync");
                return false;

            }

        }
    }
}
