using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.IRepositories
{
    public interface IAdminSettingRepository
    {
        Task<string> GetJobFrequencyAsync();
        Task<bool> UpdateUserAuthcode(string authcode);
    }
}
