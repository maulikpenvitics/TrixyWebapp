﻿
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Repository.IRepositories;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Repository.FyersWebSocketServices.Jobs
{
    public class JobSchedulerService : BackgroundService
    {
        private readonly FyersWebSocketService _fyersWebSocketService;
        private readonly IServiceProvider _serviceProvider;
        private readonly FyersStockSettings _settings;
        public JobSchedulerService(FyersWebSocketService fyersWebSocketService, IServiceProvider serviceProvider, IOptions<FyersStockSettings> settings)
        {
            _fyersWebSocketService = fyersWebSocketService;
            _serviceProvider = serviceProvider;
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var admin = scope.ServiceProvider.GetRequiredService<IAdminSettingRepository>();
                var frequnecy = await admin.GetJobFrequencyAsync();
                long time = Convert.ToInt32(frequnecy);
                RecurringJob.AddOrUpdate(
                        "delete-old-Insertnew-stock-data",
                        () => RunStockDataJob(),
                        Cron.Daily(9, 00)
                    );

                RecurringJob.AddOrUpdate(
                    "refresh-token-generate",
                     () => Refreshtokengenerate(),
                        "0 0 */13 * *"  // every 13 days
                 );
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromMinutes(time), stoppingToken);
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
                var result = await stockrepo.DeleteHistoricaldata();

                // Fetch new stock data
                var getstocks = await getstcoks();
                var stockssym = await GetSymbol();
                if (stockssym != null && stockssym.Any())
                {
                    foreach (var item in stockssym)
                    {
                        var data = await _fyersWebSocketService.FetchAndStoreHistoricalStockDataAsync(
                            sym: item ?? "",
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
                        if (getstock != null && !string.IsNullOrEmpty(getstock?.Symbol))
                        {
                            var signal = await getfinalsignal(getstock.Symbol, getstock?.userid ?? "");
                            if (!string.IsNullOrEmpty(signal))
                            {
                                await userrepo.UpdateUserStocks(getstock?.userid ?? "", getstock?.Symbol ?? "", signal);
                            }
                            if (getstock != null)
                            {
                                getstock.BuySellSignal = string.IsNullOrEmpty(signal) ? null : signal;
                            }

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
                    var assignedStocks = user.Stocks?.Where(x => adminsym.Contains(x?.Symbol ?? "") && x?.StockNotification == true).FirstOrDefault();
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

        private async Task<List<string?>> GetSymbol()
        {
            List<string?> symlst = new List<string?>();
            using (var scope = _serviceProvider.CreateScope())
            {
                var adminstocksym = scope.ServiceProvider.GetRequiredService<IRepository<StockSymbol>>();
                var stocksym = await adminstocksym.GetAllAsync();
                if (stocksym.Any() && stocksym != null)
                {
                    symlst = stocksym.Where(x => x.Status == true).Select(x => x.Symbol).ToList();
                    return symlst;
                }
            }
            return symlst;
        }
        private async Task<string> getfinalsignal(string sym, string userid)
        {
            string result = string.Empty;
            AdminSettings Adminsetting = await GetAdminsetting();
            using (var scope = _serviceProvider.CreateScope())
            {
                var historicaldata = scope.ServiceProvider.GetRequiredService<IRepository<Historical_Data>>(); // Resolve scoped service
                var data = await historicaldata.GetAllAsync();
                data = data.Where(x => x.symbol == sym).ToList();

                var user = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var userstretagy = user.GetById(userid);
                userstretagy.UserStrategy = userstretagy?.UserStrategy?.Where(x => x.StretagyEnableDisable == true && x.IsActive == true).ToList();

                if (data != null && data.Any())
                {
                    if (userstretagy?.UserStrategy != null) { result = GenrateBuysellsignal.GetCombinationsignal(Adminsetting, (List<Historical_Data>)data, userstretagy.UserStrategy); }
                }
            }
            return result;
        }

        public async Task Refreshtokengenerate()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var UserRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var adminsetting = await UserRepo.GetUserSettings();
                var authdata = adminsetting.UserAuthtoken;

                if (authdata != null)
                {
                    var getrefreshtoken = await _fyersWebSocketService.genrateRefreshtoken(authdata?.code ?? "");
                    var result = JsonSerializer.Deserialize<Accesstoken>(getrefreshtoken.ToString());
                    if (result.RESPONSE_MESSAGE== "SUCCESS")
                    {
                        
                        var updatetoken = await UserRepo.UpdateAdminbothAuthtoken(result.TOKEN, result.refresh_token);
                        _settings!.refreshtoken = updatetoken.refresh_token;
                        _settings!.clinetpin = updatetoken.Pin;
                        _settings!.AccessToken = updatetoken.access_token;
                    }
                }

            }
        }
    }
}
