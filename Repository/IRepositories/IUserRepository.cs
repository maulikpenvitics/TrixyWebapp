using Repository.Models;

namespace Repository.IRepositories
{
    public interface IUserRepository
    {
        #region User
        Task<User> GetByEmail(string Email);
        Task<int> ChangePassword(string Id, string oldpass, string newpass);
        Task<bool> UpdateUserProfile(User model);
        Task UpdateAsyncStrategy(string userId, string strategyName, bool isChecked);
        Task UpdateAsyncUserStocks(string userId, string sym, bool isChecked,string BuySellSignal);
        Task<bool> AddUserStocks(User user);
        #endregion
        #region User settings method
        Task InsertUserseting(AdminSettings model);
        Task<AdminSettings> GetUserSettings(string userId);
        Task UpdateUserSettings(string userId, AdminSettings settings);
        Task DeleteUserSettings(string userId);
        #endregion
    }
}
