using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Repository.DbModels;
using Repository.FyersWebSocketServices.Jobs;
using Repository.IRepositories;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Repository.Repositories
{
    public class UserRepository : MongoRepository<User>, IUserRepository
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<AdminSettings> _userSettingsCollection;
        private readonly IErrorHandlingRepository _errorHandlingRepository;
        public UserRepository(IMongoClient mongoClient, IOptions<MongoDBSettings> settings, IErrorHandlingRepository errorHandlingRepository) : base(mongoClient, settings, errorHandlingRepository)
        {
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _users = database.GetCollection<User>("User");
            _userSettingsCollection = database.GetCollection<AdminSettings>("AdminSettings");
            _errorHandlingRepository = errorHandlingRepository;
        }

        public async Task<User> GetByEmail(string Email)
        {
            try
            {
                return await _users.Find(Builders<User>.Filter.And(
                Builders<User>.Filter.Eq("Email", Email),
                Builders<User>.Filter.Eq("Status", true)
            )).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "UserRepository/GetByEmail");
                throw new Exception("An error occurred while retrieving data. Please try again.");
            }
            
        }
        public async Task<int> ChangePassword(string Id,string oldpass,string newpass)
        {
            try
            {
                var user = await GetByIdAsync(Id);
                if (user != null && user.Password == oldpass)
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
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "UserRepository/ChangePassword");
                throw new Exception("An error occurred while retrieving data. Please try again.");
            }
          
        }

        public async Task<bool> UpdateUserProfile(User model)
        {
            try
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
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "UserRepository/UpdateUserProfile");
                throw new Exception("An error occurred while UpdateUserProfile data. Please try again.");
            }
     
     }

        public async Task UpdateAsyncStrategy(string userId, string strategyName, bool isChecked)
        {
            try
            {
                var filter = Builders<User>.Filter.And(
                      Builders<User>.Filter.Eq("_id", ObjectId.Parse(userId)), // Match the user by ID
                      Builders<User>.Filter.ElemMatch(u => u.UserStrategy, s => s.StretagyName == strategyName));
                var update = Builders<User>.Update.Set("UserStrategy.$.StretagyEnableDisable", isChecked); // Update the StrategyEnableDisable field

                await _users.UpdateOneAsync(filter, update);
            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "UserRepository/UpdateAsyncStrategy");
                throw new Exception("An error occurred while UpdateAsyncStrategy data. Please try again.");
            }
         
        }

        public async Task UpdateAsyncUserStocks(string userId, string sym, bool isChecked,string BuySellSignal)
        {
            try
            {
                var filter = Builders<User>.Filter.And(
                         Builders<User>.Filter.Eq("_id", ObjectId.Parse(userId)),
                         Builders<User>.Filter.ElemMatch(u => u.Stocks, s => s.Symbol == sym));
                var update = Builders<User>.Update.Set("Stocks.$.StockNotification", isChecked)
                    .Set("Stocks.$.BuySellSignal", BuySellSignal);

                await _users.UpdateOneAsync(filter, update);

            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "UserRepository/UpdateAsyncUserStocks");
                throw new Exception("An error occurred while UpdateAsyncUserStocks data. Please try again.");
            }
           
        }
        public async Task UpdateUserStocks(string userId, string sym, string BuySellSignal)
        {
            try
            {
                var filter = Builders<User>.Filter.And(
                       Builders<User>.Filter.Eq("_id", ObjectId.Parse(userId)),
                       Builders<User>.Filter.ElemMatch(u => u.Stocks, s => s.Symbol == sym));
                var update = Builders<User>.Update.Set("Stocks.$.BuySellSignal", BuySellSignal)
                    .Set("Stocks.$.BuySellSignal", BuySellSignal);

                await _users.UpdateOneAsync(filter, update);
            }
            catch (Exception ex)
            {

                await _errorHandlingRepository.AddErrorHandling(ex, "UserRepository/UpdateUserStocks");
                throw new Exception("An error occurred while UpdateUserStocks data. Please try again.");
            }
          
        }
        public async Task<bool> AddUserStocks(User user)
        {
            try
            {
                var filter = Builders<User>.Filter.And(
                     Builders<User>.Filter.Eq("_id", user.Id)
                 );
                var update = Builders<User>.Update.Set("Stocks", user.Stocks);

                var result = await _users.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "UserRepository/AddUserStocks");
                throw new Exception("An error occurred while AddUserStocks data. Please try again.");
            }
        
        }

        public async Task<IEnumerable<User>> GetallUser()
        {
            try
            {
                return await GetAllAsync();
            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "UserRepository/GetallUser");
                throw new Exception("An error occurred while GetallUser data. Please try again.");
               
            }
          
        }
        public async Task<bool> updatemanyuserstock(string sym,bool? isactive)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq("Stocks.Symbol", sym);

                var update = Builders<User>.Update.Set("Stocks.$[elem].IsActive", isactive);


                var arrayFilter = new ArrayFilterDefinition<BsonDocument>[]
                {
        new BsonDocumentArrayFilterDefinition<BsonDocument>(
            new BsonDocument("elem.Symbol", sym))
                };

                // Apply the update with array filters
                var updateOptions = new UpdateOptions { ArrayFilters = arrayFilter };

                // Perform the update operation on multiple documents
                var result = await _users.UpdateManyAsync(filter, update, updateOptions);

                // Return true if any documents were modified
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "UserRepository/updatemanyuserstock");
                throw new Exception("An error occurred while updatemanyuserstock data. Please try again.");
            }
           
        }
        public User GetById(string Id)
        {
            try
            {
                if (!ObjectId.TryParse(Id, out ObjectId objectId))
                {
                    throw new ArgumentException("Invalid ObjectId format", nameof(Id));
                }
                return _users.Find(Builders<User>.Filter.Eq("_id", objectId)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                 _errorHandlingRepository.AddError(ex, "UserRepository/GetById");
                throw new Exception("An error occurred while GetById data. Please try again.");
            }
          
        }

        public async Task<bool> removeuserstock(string sym)
        {
            try
            {
                var filter = Builders<User>.Filter.And(
                         Builders<User>.Filter.ElemMatch(u => u.Stocks, s => s.Symbol == sym));
                var update = Builders<User>.Update.Set("Stocks.$.IsActive", false);
                  
                var result = await _users.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;

                //var filter = Builders<User>.Filter.ElemMatch(u => u.Stocks, s => s.Symbol == sym);
                //var update = Builders<User>.Update.PullFilter(u => u.Stocks, s => s.Symbol == sym);

                //var result = await _users.UpdateOneAsync(filter, update);
                //return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "UserRepository/removeuserstock");
                //throw new Exception("An error occurred while removeuserstock data. Please try again.");
                return false;
            }
        }

        #region Adminswttings
        public async Task InsertUserseting(AdminSettings model)
        {
            try
            {
                await _userSettingsCollection.InsertOneAsync(model);
            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "UserRepository/InsertUserseting");
                throw new Exception("An error occurred while InsertUserseting data. Please try again.");
            }

        }
        public async Task<AdminSettings> GetUserSettings()
        {
            try
            {
                var settings = await _userSettingsCollection
          .Find(_ => true)
         .FirstOrDefaultAsync();

                return settings ?? new AdminSettings(); //
            }
            catch (Exception ex)
            {

                await _errorHandlingRepository.AddErrorHandling(ex, "UserRepository/GetUserSettings");
                throw new Exception("An error occurred while GetUserSettings data. Please try again.");
            }
          
        }
        public async Task UpdateUserSettings(string userId, AdminSettings settings)
        {
            try
            {
                await _userSettingsCollection.ReplaceOneAsync(s => s.UserId == userId, settings);
            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "UserRepository/UpdateUserSettings");
                throw new Exception("An error occurred while UpdateUserSettings data. Please try again.");
            }
           
        }
        public async Task DeleteUserSettings(string userId)
        {
            try
            {
                await _userSettingsCollection.DeleteOneAsync(s => s.UserId == userId);

            }
            catch (Exception ex)
            {

                await _errorHandlingRepository.AddErrorHandling(ex, "UserRepository/DeleteUserSettings");
                throw new Exception("An error occurred while DeleteUserSettings data. Please try again.");
            }
            
        }

        public  async Task<UserAuthtoken> UpdateAdminAuthtoken(string accesstoken)
        {
            try
            {
                if (!string.IsNullOrEmpty(accesstoken))
                {
                    var filter = Builders<AdminSettings>.Filter.Empty;

                    var update = Builders<AdminSettings>.Update.Set(x => x.UserAuthtoken.access_token, accesstoken);

                    var updatedata = await _userSettingsCollection.UpdateOneAsync(filter, update);
                    if (updatedata.ModifiedCount > 0)
                    {
                        var admin = await GetUserSettings();
                        return admin.UserAuthtoken;
                    }
                }
                return new UserAuthtoken();
            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "UserRepository/UpdateAdminAuthtoken");
                throw new Exception("An error occurred while UpdateAdminAuthtoken data. Please try again.");
            }
           

        }

        public async Task<UserAuthtoken> UpdateAdminbothAuthtoken(string accesstoken,string refreshtoken)
        {
            try
            {
                if (!string.IsNullOrEmpty(accesstoken))
                {
                    var filter = Builders<AdminSettings>.Filter.Empty;

                    var update = Builders<AdminSettings>.Update
                        .Set(x => x.UserAuthtoken.access_token, accesstoken)
                        .Set(x => x.UserAuthtoken.refresh_token, refreshtoken);

                    var updatedata = await _userSettingsCollection.UpdateOneAsync(filter, update);
                    if (updatedata.ModifiedCount > 0)
                    {
                        var admin = await GetUserSettings();
                        return admin.UserAuthtoken;
                    }
                }
                return new UserAuthtoken();

            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "UserRepository/UpdateAdminbothAuthtoken");
                throw new Exception("An error occurred while UpdateAdminbothAuthtoken data. Please try again.");
            }
            
        }
       
        #endregion



    }
}
