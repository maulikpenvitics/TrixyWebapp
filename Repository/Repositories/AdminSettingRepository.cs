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
    public  class AdminSettingRepository : MongoRepository<AdminSettings>, IAdminSettingRepository
    {
        private readonly IMongoCollection<AdminSettings> _adminSettings;
        public AdminSettingRepository(IMongoClient mongoClient, IOptions<MongoDBSettings> settings) : base(mongoClient, settings)
        {
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _adminSettings = database.GetCollection<AdminSettings>("AdminSettings");
        }
    }
}
