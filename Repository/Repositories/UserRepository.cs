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
    public class UserRepository : MongoRepository<User>, IUserRepository
    {
        private readonly IMongoCollection<User> _users;
        public UserRepository(IMongoClient mongoClient, IOptions<MongoDBSettings> settings) : base(mongoClient, settings)
        {
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _users = database.GetCollection<User>("user");
        }

        public async Task<User> GetByEmail(string Email)
        {
            return await _users.Find(Builders<User>.Filter.And(
                Builders<User>.Filter.Eq("Email", Email),
                Builders<User>.Filter.Eq("Status", true)
            )).FirstOrDefaultAsync();
        }
    }
}
