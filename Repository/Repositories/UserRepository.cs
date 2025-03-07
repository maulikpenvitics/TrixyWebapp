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
    public class UserRepository : MongoRepository<User>, IUserRepository
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<AdminSettings> _userSettingsCollection;
        public UserRepository(IMongoClient mongoClient, IOptions<MongoDBSettings> settings) : base(mongoClient, settings)
        {
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _users = database.GetCollection<User>("User");
            _userSettingsCollection = database.GetCollection<AdminSettings>("AdminSettings");
        }

        public async Task<User> GetByEmail(string Email)
        {
            return await _users.Find(Builders<User>.Filter.And(
                Builders<User>.Filter.Eq("Email", Email),
                Builders<User>.Filter.Eq("Status", true)
            )).FirstOrDefaultAsync();
        }
        public async Task<int> ChangePassword(string Id,string oldpass,string newpass)
        {
            var user= await GetByIdAsync(Id);
            if (user !=null && user.Password==oldpass)
            {
                var update = Builders<User>.Update
            .Set(u => u.Password, newpass)
            .Set(u => u.UpdatedDate, DateTime.UtcNow);
                var objid = ObjectId.TryParse(Id, out ObjectId objectId);
                    var result = await _users.UpdateOneAsync(
                    Builders<User>.Filter.Eq("_id", objectId), update);

                return result.ModifiedCount > 0 ? 1 : 0;
                
            }
            else
            {
                return 0;
            }
           
        }

        public async Task<bool> UpdateUserProfile(User model)
      {
        if (model == null)
            return false; 
        var filter = Builders<User>.Filter.Eq("_id", model.Id); 

        var update = Builders<User>.Update
            .Set(u => u.Firstname, model.Firstname)
            .Set(u => u.Lastname, model.Lastname)
            .Set(u => u.Email, model.Email)
            .Set(u => u.ProfileImageUrl, model.ProfileImageUrl)
            .Set(u => u.UpdatedDate, DateTime.UtcNow); 

        var result = await _users.UpdateOneAsync(filter, update);

        return result.ModifiedCount > 0; 
     }

        public async Task UpdateAsyncStrategy(string userId, string strategyName, bool isChecked)
        {
            var filter = Builders<User>.Filter.And(
                         Builders<User>.Filter.Eq("_id", ObjectId.Parse(userId)), // Match the user by ID
                         Builders<User>.Filter.ElemMatch(u=>u.UserStrategy,s=>s.StretagyName==strategyName));
            var update = Builders<User>.Update.Set("UserStrategy.$.StretagyEnableDisable", isChecked); // Update the StrategyEnableDisable field

            await _users.UpdateOneAsync(filter, update);
        }

        public async Task UpdateAsyncUserStocks(string userId, string sym, bool isChecked)
        {
            var filter = Builders<User>.Filter.And(
                         Builders<User>.Filter.Eq("_id", ObjectId.Parse(userId)), // Match the user by ID
                         Builders<User>.Filter.ElemMatch(u => u.Stocks, s => s.Symbol == sym));
            var update = Builders<User>.Update.Set("Stocks.$.StockNotification", isChecked); // Update the StrategyEnableDisable field

            await _users.UpdateOneAsync(filter, update);
        }

        #region Adminswttings
        public async Task InsertUserseting(AdminSettings model)
        {
            await _userSettingsCollection.InsertOneAsync(model);
        }
        public async Task<AdminSettings> GetUserSettings(string userId)
        {
            return await _userSettingsCollection.Find(s => s.UserId == userId).FirstOrDefaultAsync();
        }
        public async Task UpdateUserSettings(string userId, AdminSettings settings)
        {
            await _userSettingsCollection.ReplaceOneAsync(s => s.UserId == userId, settings);
        }
        public async Task DeleteUserSettings(string userId)
        {
            await _userSettingsCollection.DeleteOneAsync(s => s.UserId == userId);
        }
        #endregion

    

    }
}
