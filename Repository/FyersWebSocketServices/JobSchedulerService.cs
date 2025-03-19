
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Repository.IRepositories;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.FyersWebSocketServices
{
    public class JobSchedulerService //: BackgroundService
    {
        private readonly FyersWebSocketService _fyersWebSocketService;
        private readonly IServiceProvider _serviceProvider;
      //  private string _lastFrequency = Cron.Daily();

        public JobSchedulerService(FyersWebSocketService fyersWebSocketService, IServiceProvider serviceProvider)
        {
            _fyersWebSocketService = fyersWebSocketService;
            _serviceProvider = serviceProvider;
        }

        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    while (!stoppingToken.IsCancellationRequested)
        //    {
        //        using (var scope = _serviceProvider.CreateScope())
        //        {
        //            var adminSettingRepo = scope.ServiceProvider.GetRequiredService<IAdminSettingRepository>();
        //            var newFrequency = await adminSettingRepo.GetJobFrequencyAsync();

        //            if (_lastFrequency != newFrequency)
        //            {
        //                RecurringJob.AddOrUpdate(
        //                    "delete-old-Insertnew-stock-data",
        //                    () => RunStockDataJob(),
        //                    Cron.Hourly
        //                );

        //                _lastFrequency = newFrequency;
        //            }
        //        }

        //        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        //    }
        //}
       // [AutomaticRetry(Attempts = 3)] // Optional: Retry if the job fails
        //public async Task RunStockDataJob()
        //{
        //    using (var scope = _serviceProvider.CreateScope())
        //    {
        //        var stockrepo = scope.ServiceProvider.GetRequiredService<IWebStockRepository>();
        //        var userrepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        //        // Delete old historical data
        //        await stockrepo.DeleteHistoricaldata();

        //        // Fetch new stock data
        //        var getstocks = await getstcoks();
        //        if (getstocks?.Any() == true)
        //        {
        //            // Insert distinct stocks
        //            var insertgetstock = getstocks.DistinctBy(x => x.Symbol).ToList();
        //            foreach (var item in insertgetstock)
        //            {
        //                var data = await _fyersWebSocketService.FetchAndStoreHistoricalStockDataAsync(
        //                    item?.Symbol,
        //                    DateTime.UtcNow.Date.AddDays(-2).ToString("yyyy-MM-dd"),
        //                    DateTime.UtcNow.Date.ToString("yyyy-MM-dd")
        //                );

        //                await stockrepo.InsertNewHistoricalData(data);
        //            }
        //            foreach (var getstock in getstocks)
        //            {
        //                if (!string.IsNullOrEmpty(getstock?.Symbol))
        //                {
        //                    var signal = await getfinalsignal(getstock.Symbol);
        //                    if (!string.IsNullOrEmpty(signal))
        //                    {
        //                       await  userrepo.UpdateAsyncUserStocks(getstock?.userid,getstock?.Symbol,signal);
        //                    }
        //                    getstock.BuySellSignal = string.IsNullOrEmpty(signal) ? null : signal;
        //                }
        //            }
        //        }
        //    }
        //}
        //private async Task<List<Stocknotifactiondata>> getstcoks()
        //{
        //    List<Stocknotifactiondata> stocks = new List<Stocknotifactiondata>();
        //    using (var scope = _serviceProvider.CreateScope())
        //    {
        //        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>(); // Resolve scoped service
        //        var users = await userRepository.GetallUser();
        //        foreach (var user in users)
        //        {
        //            var assignedStocks = user.Stocks?.Where(x => x.StockNotification == true).ToList() ?? new List<Stocks>();
        //            if (assignedStocks != null)
        //            {
        //                foreach (var item in assignedStocks)
        //                {
        //                    stocks.Add(new Stocknotifactiondata()
        //                    {
        //                        BuySellSignal = item.BuySellSignal,
        //                        Change = 0,
        //                        Price = 0,
        //                        Priviscloseprice = 0,
        //                        Symbol = item.Symbol,
        //                        userid = user.Id.ToString(),
        //                    });

        //                }

        //            }
        //        }
        //        return stocks;
        //    }
        //}

        //private async Task<AdminSettings> GetAdminsetting()
        //{
        //    AdminSettings adminSettings = new AdminSettings();
        //    using (var scope = _serviceProvider.CreateScope())
        //    {
        //        var Admindsetting = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        //        adminSettings = await Admindsetting.GetUserSettings();
        //        return adminSettings;
        //    }
        //}

        //private async Task<string> getfinalsignal(string sym)
        //{
        //    string result = string.Empty;
        //    AdminSettings Adminsetting = await GetAdminsetting();
        //    using (var scope = _serviceProvider.CreateScope())
        //    {
        //        var historicaldata = scope.ServiceProvider.GetRequiredService<IRepository<Historical_Data>>(); // Resolve scoped service
        //        var data = await historicaldata.GetAllAsync();
        //        data = data.Where(x => x.symbol == sym).ToList();
        //        if (data != null && data.Any())
        //        {
        //            result = GenrateBuysellsignal.GetCombinationsignal(Adminsetting, (List<Historical_Data>)data);
        //        }
        //    }
        //    return result;
        //}
    }
}
