using Repository.BusinessLogic;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.IRepositories
{
    public interface IWebStockRepository
    {
       Task<List<Historical_Data>> GetHistoricalData();
    }
}
