﻿using Microsoft.Extensions.Options;
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
    public class MongoRepository<T> : IRepository<T> where T : class
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<T> _collection;

        public MongoRepository(IMongoClient mongoClient, IOptions<MongoDBSettings> settings)
        {
            _database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _collection = _database.GetCollection<T>(typeof(T).Name);
        }

        public async Task<T> AuthenticateUserAsync(string email, string password) =>
        await _collection.Find(Builders<T>.Filter.And(
            Builders<T>.Filter.Eq("email", email),
            Builders<T>.Filter.Eq("password", password)
        )).FirstOrDefaultAsync();


        public async Task<IEnumerable<T>> GetAllAsync() =>
        await _collection.Find(_ => true).ToListAsync();

        public async Task<T> GetByIdAsync(string id) =>
            await _collection.Find(Builders<T>.Filter.Eq("_id", id)).FirstOrDefaultAsync();

        public async Task<T> GetByIdAsyncForMaster(string userId) =>
            await _collection.Find(Builders<T>.Filter.Eq("userId", userId)).FirstOrDefaultAsync();

        public async Task CreateAsync(T entity) =>
            await _collection.InsertOneAsync(entity);

        public async Task UpdateAsync(string id, T entity) =>
            await _collection.ReplaceOneAsync(Builders<T>.Filter.Eq("_id", id), entity);

        public async Task UpdateAsyncStrategy(string userId, string strategyName, bool isChecked)
        {
            var filter = Builders<T>.Filter.Eq("userId", userId); // Filter only by userId

            var update = Builders<T>.Update.Set(strategyName, isChecked); // Dynamically update the field

            await _collection.UpdateOneAsync(filter, update);
        }

        public async Task DeleteAsync(string id) =>
            await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", id));

        public async  Task InsertManyAsync(List<T> entities)
        {
            if (entities != null)
            {
                await _collection.InsertManyAsync(entities);
            }
        }
    }
}
