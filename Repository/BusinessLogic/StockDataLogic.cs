using Repository.IRepositories;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.BusinessLogic
{
    public static class StockDataLogic
    {
        public static List<AverageCrossoverStockData> AverageCrossoverStrategy(List<Historical_Data> historical_Datas)
        {
            if (historical_Datas !=null && historical_Datas.Any())
            {
                List<AverageCrossoverStockData> crossoverStockDatas = (List<AverageCrossoverStockData>)historical_Datas
                                                             .Select(h => new AverageCrossoverStockData
                                                             {
                                                                 Close = h.Close,
                                                                 Signal = null,
                                                                 SMA_10 = null,
                                                                 SMA_50 = null,
                                                                 Timestamp = h.Timestamp,
                                                             });

                for (int i = 0; i < crossoverStockDatas.Count; i++)
                {
                    if (i >= 9)
                        crossoverStockDatas[i].SMA_10 = (double?)crossoverStockDatas.Skip(i - 9).Take(10).Average(x => x.Close);

                    if (i >= 49)
                        crossoverStockDatas[i].SMA_50 = (double?)crossoverStockDatas.Skip(i - 49).Take(50).Average(x => x.Close);
                }
                for (int i = 1; i < crossoverStockDatas.Count; i++)
                {
                    if (crossoverStockDatas[i].SMA_10.HasValue && crossoverStockDatas[i].SMA_50.HasValue)
                    {
                        if (crossoverStockDatas[i - 1].SMA_10 <= crossoverStockDatas[i - 1].SMA_50 &&
                            crossoverStockDatas[i].SMA_10 > crossoverStockDatas[i].SMA_50)
                        {
                            crossoverStockDatas[i].Signal = "BUY";
                        }
                        else if (crossoverStockDatas[i - 1].SMA_10 >= crossoverStockDatas[i - 1].SMA_50 &&
                                 crossoverStockDatas[i].SMA_10 < crossoverStockDatas[i].SMA_50)
                        {
                            crossoverStockDatas[i].Signal = "SELL";
                        }
                        else
                        {
                            crossoverStockDatas[i].Signal = "HOLD";
                        }
                    }
                }

                return crossoverStockDatas;
            }
            return new List<AverageCrossoverStockData>(); 
        }
    }
    public class AverageCrossoverStockData
    {
        public DateTime Timestamp { get; set; }
        public decimal? Close { get; set; }
        public double? SMA_10 { get; set; }
        public double? SMA_50 { get; set; }
        public string? Signal { get; set; }
    }
}
