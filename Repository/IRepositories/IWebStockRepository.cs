
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
   
        Task<List<Historical_Data>> GetStockDataBySymbolAsync(string symbol);
        Task<int> DeleteHistoricaldata();
        Task InsertNewHistoricalData(List<Historical_Data> newData);
        Task<int> everydayDeleteHistoricaldata();
    }
}
