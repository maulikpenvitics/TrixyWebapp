using MongoDB.Driver;
using Repository.BusinessLogic;
using Repository.IRepositories;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class WebStockRepository : IWebStockRepository
    {
        private readonly IRepository<Historical_Data> _collection;
        public WebStockRepository(IRepository<Historical_Data> collection)
        {
            _collection = collection;
        }
        public async Task<List<Historical_Data>> GetHistoricalData()
        {
            List<Historical_Data> historical_Datas = new List<Historical_Data>();
            historical_Datas = (List<Historical_Data>)await _collection.GetAllAsync();
            return historical_Datas;
        }
    }
}
