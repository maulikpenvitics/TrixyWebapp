﻿using Repository.Models;

namespace Repository.IRepositories
{
    public interface IUserRepository
    {
        #region User
        Task<User> GetByEmail(string Email);
        Task<int> ChangePassword(string Id, string oldpass, string newpass);
        Task<bool> UpdateUserProfile(User model);
        #endregion
        #region User settings method
        Task InsertUserseting(AdminSettings model);
        Task<AdminSettings> GetUserSettings(string userId);
        Task UpdateUserSettings(string userId, AdminSettings settings);
        Task DeleteUserSettings(string userId);
        #endregion
    }
}
