
using Hangfire;
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
    public class JobSchedulerService : BackgroundService
    {
        private readonly FyersWebSocketService _fyersWebSocketService;
        private readonly IServiceProvider _serviceProvider;
        private string _lastFrequency = Cron.Daily();

        public JobSchedulerService(FyersWebSocketService fyersWebSocketService, IServiceProvider serviceProvider)
        {
            _fyersWebSocketService = fyersWebSocketService;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var adminSettingRepo = scope.ServiceProvider.GetRequiredService<IAdminSettingRepository>();
                    var newFrequency = await adminSettingRepo.GetJobFrequencyAsync();
                    var cron = Convert.ToInt32(newFrequency);
                    //RecurringJob.AddOrUpdate(
                    //       "delete-old-Insertnew-stock-data",
                    //       () => RunStockDataJob(),
                    //       Cron.Minutely
                    //   );

                    if (_lastFrequency != newFrequency)
                    {
                        RecurringJob.AddOrUpdate(
                            "delete-old-Insertnew-stock-data",
                            () => RunStockDataJob(),
                            Cron.Daily
                        );

                        _lastFrequency = newFrequency;
                    }
                    await Task.Delay(TimeSpan.FromDays(cron), stoppingToken);
                }

              
            }
        }
        [AutomaticRetry(Attempts = 3)] // Optional: Retry if the job fails
        public async Task RunStockDataJob()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var stockrepo = scope.ServiceProvider.GetRequiredService<IWebStockRepository>();
                var userrepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                // Delete old historical data
                await stockrepo.DeleteHistoricaldata();

                // Fetch new stock data
                var getstocks = await getstcoks();
                var stockssym = await GetSymbol();
                if (stockssym!=null && stockssym.Any())
                {
                    foreach (var item in stockssym)
                    {
                        var data = await _fyersWebSocketService.FetchAndStoreHistoricalStockDataAsync(
                            item,
                            DateTime.UtcNow.Date.AddDays(-1).ToString("yyyy-MM-dd"),
                            DateTime.UtcNow.Date.ToString("yyyy-MM-dd")
                        );
                        await stockrepo.InsertNewHistoricalData(data);
                    }
                }
                if (getstocks?.Any() == true)
                {
                    foreach (var getstock in getstocks)
                    {
                        if (!string.IsNullOrEmpty(getstock?.Symbol))
                        {
                            var signal = await getfinalsignal(getstock.Symbol,getstock.userid);
                            if (!string.IsNullOrEmpty(signal))
                            {
                                await userrepo.UpdateAsyncUserStocks(getstock?.userid, getstock?.Symbol, signal);
                            }
                            getstock.BuySellSignal = string.IsNullOrEmpty(signal) ? null : signal;
                        }
                    }
                }
            }
        }
        private async Task<List<Stocknotifactiondata>> getstcoks()
        {
            List<Stocknotifactiondata> stocks = new List<Stocknotifactiondata>();
            using (var scope = _serviceProvider.CreateScope())
            {
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>(); // Resolve scoped service
                var users = await userRepository.GetallUser();
                var adminsym = await GetSymbol();
                foreach (var user in users)
                {
                    //var assignedStocks = user.Stocks?.Where(x => x.StockNotification == true).ToList() ?? new List<Stocks>();
                    var assignedStocks = user.Stocks?.Where(x => adminsym.Contains(x?.Symbol??"") && x?.StockNotification == true).FirstOrDefault();
                    if (assignedStocks != null)
                    {
                        stocks.Add(new Stocknotifactiondata()
                        {
                            BuySellSignal = assignedStocks.BuySellSignal,
                            Change = 0,
                            Price = 0,
                            Priviscloseprice = 0,
                            Symbol = assignedStocks.Symbol,
                            userid = user.Id.ToString(),
                        });
                    }
                }
                return stocks;
            }
        }

        private async Task<AdminSettings> GetAdminsetting()
        {
            AdminSettings adminSettings = new AdminSettings();
            using (var scope = _serviceProvider.CreateScope())
            {
                var Admindsetting = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                adminSettings = await Admindsetting.GetUserSettings();
                return adminSettings;
            }
        }

        private async Task<List<string>> GetSymbol()
        { 
            List<string> symlst= new List<string>();
            using (var scope = _serviceProvider.CreateScope())
            {
                var adminstocksym = scope.ServiceProvider.GetRequiredService<IRepository<StockSymbol>>();
                var stocksym=await adminstocksym.GetAllAsync();
                if (stocksym.Any() && stocksym!=null)
                {
                    symlst = stocksym.Where(x => x.Status == true).Select(x => x.Symbol).ToList();
                    return symlst;
                }
            }
            return symlst;
        }
        private async Task<string> getfinalsignal(string sym,string userid)
        {
            string result = string.Empty;
            AdminSettings Adminsetting = await GetAdminsetting();
            using (var scope = _serviceProvider.CreateScope())
            {
                var historicaldata = scope.ServiceProvider.GetRequiredService<IRepository<Historical_Data>>(); // Resolve scoped service
                var data = await historicaldata.GetAllAsync();
                data = data.Where(x => x.symbol == sym).ToList();

                var user=scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var userstretagy = user.GetById(userid);
                userstretagy.UserStrategy= userstretagy?.UserStrategy?.Where(x=>x.StretagyEnableDisable==true && x.IsActive==true).ToList();

                if (data != null && data.Any())
                {
                    result = GenrateBuysellsignal.GetCombinationsignal(Adminsetting, (List<Historical_Data>)data, userstretagy.UserStrategy);
                }
            }
            return result;
        }
    }
}
