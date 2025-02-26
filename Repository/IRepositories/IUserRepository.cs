using Repository.Models;

namespace Repository.IRepositories
{
    public interface IUserRepository
    {
        Task<User> GetByEmail(string Email);
    }
}
