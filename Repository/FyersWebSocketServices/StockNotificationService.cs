﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Repository.FyersWebSocketServices.Jobs;
using Repository.Hubs;
using Repository.IRepositories;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Repository.FyersWebSocketServices
{
    public class StockNotificationService : BackgroundService
    {
        private readonly IHubContext<StockNotificationHub> _hubContext;
        private readonly FyersWebSocketService _fyersWebSocketService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StockNotificationService> _logger;
        public StockNotificationService(IHubContext<StockNotificationHub> hubContext,
            FyersWebSocketService fyersWebSocketService,
            IServiceProvider serviceProvider, ILogger<StockNotificationService> logger)
        {
            _hubContext = hubContext;
            _fyersWebSocketService = fyersWebSocketService;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var admin = scope.ServiceProvider.GetRequiredService<IAdminSettingRepository>();
                        var frequnecy = await admin.GetJobFrequencyAsync();
                        long time = Convert.ToInt32(frequnecy);
                        var marketStart = new TimeSpan(9, 15, 0);
                        var marketEnd = new TimeSpan(15, 30, 0);
                        var currentTime = DateTime.Now.TimeOfDay;
                        var getstockdata = await getstcoks();
                        var onlineUsers = StockNotificationHub.GetConnectedUsers();


                        foreach (var item in onlineUsers)
                        {
                            if (getstockdata != null && getstockdata.Any())
                            {
                                await _hubContext.Clients.User(item).SendAsync("ReceiveStockUpdate", getstockdata);
                            }
                        }
                        try
                        {
                            await Task.Delay(TimeSpan.FromMinutes(time), stoppingToken);
                        }
                        catch (TaskCanceledException)
                        {
                            _logger.LogInformation("JobSchedulerService task was canceled during delay.");
                            // Optional: break the loop explicitly if desired
                            break;
                        }

                       
                    }

                }
            }
            catch (Exception ex)
            {
                 Errorhandl(ex, "StockNotificationService/ExecuteAsync");
                throw;
            }
          
        }
        private List<StockData> GetLiveStockPrice(List<Stocks> symbol)
        {
            try
            {
                if (symbol != null)
                {
                    List<StockData> stockData = _fyersWebSocketService.GetStockData();
                    return stockData;
                }
                else
                {
                    return new List<StockData>();
                }
            }
            catch (Exception ex)
            {
                 Errorhandl(ex, "StockNotificationService/GetLiveStockPrice");
                return new List<StockData>();
            }
           

        }

        private async Task<List<Stocknotifactiondata>> getstcoks()
        {
            List<Stocknotifactiondata> stocks = new List<Stocknotifactiondata>();
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var userId = StockNotificationHub.GetLoggedInUserId();
                    if (string.IsNullOrEmpty(userId))
                    {
                        Console.WriteLine("User ID not found in claims.");
                        return stocks;
                    }
                    if (userId != null)
                    {
                        var userRepository = scope.ServiceProvider.GetRequiredService<IRepository<User>>(); // Resolve scoped service
                        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>(); // Resolve scoped service
                        var users = await userRepository.GetByIdAsync(userId);
                        var assignedStocks = users.Stocks?.Where(x => x.StockNotification == true && x.IsActive==true).ToList() ?? new List<Stocks>();
                        List<string> userstrategy = users.UserStrategy != null ? users.UserStrategy.Where(x => x.StretagyEnableDisable == true).Select(x => x.StretagyName ?? "").ToList() : new List<string>();
                        if (assignedStocks != null && assignedStocks.Any())
                        {
                            users.UserStrategy = users.UserStrategy != null ? users.UserStrategy.Where(x => x.StretagyEnableDisable == true && x.IsActive == true).ToList() : new List<UserStrategy>();
                            var stockPrices = GetLiveStockPrice(assignedStocks);
                            foreach (var item in assignedStocks)
                            {
                                // var signal = users?.UserStrategy!=null? await getfinalsignal(item?.Symbol??"", users.UserStrategy):string.Empty;
                                //if (!string.IsNullOrEmpty(signal))
                                //{
                                //    await userRepo.UpdateUserStocks(users?.Id.ToString()??"", item?.Symbol ?? "", signal);
                                //}
                                stocks.Add(new Stocknotifactiondata()
                                {
                                    BuySellSignal = item.BuySellSignal,
                                    Change = stockPrices.Where(x => x.Symbol == item?.Symbol)?.FirstOrDefault()?.Change,
                                    Price = stockPrices.Where(x => x.Symbol == item?.Symbol)?.FirstOrDefault()?.Price,
                                    Priviscloseprice = stockPrices.Where(x => x.Symbol == item?.Symbol)?.FirstOrDefault()?.prev_close_price,
                                    Symbol = item?.Symbol,
                                    userid = users?.Id.ToString(),
                                    timestamp = DateTime.Now,
                                    CompanyName = item?.CompanyName,
                                    userStrategy = userstrategy.Count() != 0 ? userstrategy : new List<string>()
                                });

                            }

                        }
                        return stocks;
                    }

                }
                return stocks;
            }
            catch (Exception ex)
            {
                Errorhandl(ex, "StockNotificationService/getstcoks");
                return stocks;
            }
           
        }

        private async Task<AdminSettings> GetAdminsetting()
        {
            AdminSettings adminSettings = new AdminSettings();
            try
            {
              
                using (var scope = _serviceProvider.CreateScope())
                {
                    var Admindsetting = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                    adminSettings = await Admindsetting.GetUserSettings();
                    return adminSettings;
                }
            }
            catch (Exception ex)
            {
                Errorhandl(ex, "StockNotificationService/GetLiveStockPrice");
                return adminSettings;
            }
           
        }

        private async Task<string> getfinalsignal(string sym,List<UserStrategy> strategy)
        {
            string result=string.Empty;
            AdminSettings Adminsetting =await GetAdminsetting();
            using (var scope = _serviceProvider.CreateScope())
            {
                var historicaldata = scope.ServiceProvider.GetRequiredService<IRepository<Historical_Data>>(); // Resolve scoped service
              
                var data = await historicaldata.GetAllAsync();
                var symdata= data.Where(x=>x.symbol == sym).ToList();
                if (symdata != null && symdata.Any())
                {
                  result = GenrateBuysellsignal.GetCombinationsignal(Adminsetting,symdata, strategy);
                }
            }
            return result;
        }


        public void  Errorhandl(Exception ex, string remarks)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var errorrepo = scope.ServiceProvider.GetRequiredService<IErrorHandlingRepository>();
                     errorrepo.AddError(ex, remarks);
                }

            }
            catch (Exception innerex)
            {
                _logger.LogError(innerex, "JobSchedulerService/Errorhandl");
            }

        }
    }
}
