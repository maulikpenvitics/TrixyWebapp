using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Repository.DbModels;
using Repository.IRepositories;
using Repository.Models;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class MongoRepository<T> : IRepository<T> where T : class
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<T> _collection;
        private readonly IErrorHandlingRepository _errorHandlingRepository;
    
        public MongoRepository(IMongoClient mongoClient, IOptions<MongoDBSettings> settings, IErrorHandlingRepository errorHandlingRepository)
        {
            _database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _collection = _database.GetCollection<T>(typeof(T).Name);
            _errorHandlingRepository = errorHandlingRepository;
        }
        public async Task<T> AuthenticateUserAsync(string email, string password)
        {
            try
            {
                return await _collection.Find(Builders<T>.Filter.And(
               Builders<T>.Filter.Eq("Email", email),
               Builders<T>.Filter.Eq("Password", password),
               Builders<T>.Filter.Eq("Status", true)
              )).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "MongoRepository/AuthenticateUserAsync");
                throw new Exception("An error occurred while retrieving data. Please try again.");
            }
         
        }

        public async Task<T> getUserByEmail(string email)
        {
            try
            {
                return await _collection.Find(Builders<T>.Filter.And(
              Builders<T>.Filter.Eq("Email", email),
              Builders<T>.Filter.Eq("Status", true)
                  )).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "MongoRepository/getUserByEmail");
                throw new Exception("An error occurred while retrieving data. Please try again.");
            }
        
        }
        public async Task<IEnumerable<T>> GetAllAsync() {
            try
            {
                return await _collection.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "MongoRepository/GetAllAsync");
                throw new Exception("An error occurred while retrieving data. Please try again.");
            }
         
        }
        

        public async Task<T> GetByIdAsync(string id)
        {
            try
            {
                if (!ObjectId.TryParse(id, out ObjectId objectId))
                {
                    throw new ArgumentException("Invalid ObjectId format", nameof(id));
                }
                return await _collection.Find(Builders<T>.Filter.Eq("_id", objectId)).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "MongoRepository/GetByIdAsync");
                throw new Exception("An error occurred while retrieving data. Please try again.");
            }
         
        }


        public async Task<T> GetByIdAsyncForMaster(string userId)
        {
            try
            {
                return await _collection.Find(Builders<T>.Filter.Eq("userId", userId)).FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "MongoRepository/GetByIdAsyncForMaster");
                throw new Exception("An error occurred while retrieving data. Please try again.");
            }
        }

        public async Task CreateAsync(T entity)
        {
            try
            {
                await _collection.InsertOneAsync(entity);
            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "MongoRepository/CreateAsync");
                throw new Exception("An error occurred while create data. Please try again.");
            }
           
        }

        public async Task UpdateAsync(string id, T entity)
        {
            try
            {
                if (!ObjectId.TryParse(id, out ObjectId objectId))
                {
                    throw new ArgumentException("Invalid ObjectId format", nameof(id));
                }
                await _collection.ReplaceOneAsync(Builders<T>.Filter.Eq("_id", objectId), entity);
            }
            catch (Exception ex)
            {

                await _errorHandlingRepository.AddErrorHandling(ex, "MongoRepository/UpdateAsync");
                throw new Exception("An error occurred while Update data. Please try again.");
            }
           
        }



        public async Task DeleteAsync(string id)
        {
            try
            {
                await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", id));
            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "MongoRepository/DeleteAsync");
                throw new Exception("An error occurred while Delete data. Please try again.");
            }
           
        }
        public async  Task InsertManyAsync(List<T> entities)
        {
            try
            {
                if (entities != null)
                {
                    await _collection.InsertManyAsync(entities);
                }
            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "MongoRepository/InsertManyAsync");
                throw new Exception("An error occurred while InsertMany data. Please try again.");
            }
           
        }

        public async Task<int> InsertAsync(T entity)
        {
           
            try
            {
                await _collection.InsertOneAsync(entity);
                return 1;
            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "MongoRepository/DeleteAsync");
                
                return 0;
            }
        }

        public async Task<List<T>> FindAsync(FilterDefinition<T> filter)
        {
            try
            {
                return await _collection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorHandlingRepository.AddErrorHandling(ex, "MongoRepository/FindAsync");
                throw new Exception("An error occurred while Find data. Please try again.");
            }
           
        }

    }
}
