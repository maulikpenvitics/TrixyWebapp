using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.IRepositories
{
    public interface IErrorHandlingRepository
    {
        Task AddErrorHandling(Exception ex, string remarks);
        void AddError(Exception ex, string remarks);
        Task<IEnumerable<ErrorHandling>> GetAllErrorsAsync();
    }
}
