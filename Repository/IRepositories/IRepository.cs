using MongoDB.Bson;
using MongoDB.Driver;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.IRepositories
{
    public interface IRepository<T>
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AuthenticateUserAsync(string email, string password);
        Task<T> GetByIdAsync(string id);
        Task<T> GetByIdAsyncForMaster(string userId);
        Task CreateAsync(T entity);
        Task UpdateAsync(string id, T entity);
        Task DeleteAsync(string id);
        Task InsertManyAsync(List<T> entities);
        Task UpdateAsyncStrategy(string userId, string strategyName, bool isChecked);
        Task<int> InsertAsync(T entity);
        Task<List<T>> FindAsync(FilterDefinition<T> filter);
    }
}
