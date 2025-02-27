using Repository.Models;

namespace Repository.IRepositories
{
    public interface IUserRepository
    {
        Task<User> GetByEmail(string Email);
        Task<int> ChangePassword(string Id, string oldpass, string newpass);
        Task<bool> UpdateUserProfile(User model);
    }
}
