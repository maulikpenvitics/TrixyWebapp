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
        Task CreateAsync(T entity);
        Task UpdateAsync(string id, T entity);
        Task DeleteAsync(string id);
        Task InsertManyAsync(List<T> entities);
        Task <int> InsertAsync(T entity);
        Task<T> getUserByEmail(string email);
    }
}
