using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
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
        public StockNotificationService(IHubContext<StockNotificationHub> hubContext,
            FyersWebSocketService fyersWebSocketService,
            IServiceProvider serviceProvider)
        {
            _hubContext = hubContext;
            _fyersWebSocketService = fyersWebSocketService;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                using (var scope = _serviceProvider.CreateScope())
                {
                    var admin = scope.ServiceProvider.GetRequiredService<IAdminSettingRepository>();
                    var frequnecy =await admin.GetJobFrequencyAsync();
                    long time =Convert.ToInt32(frequnecy);
                    var marketStart = new TimeSpan(9, 15, 0);
                    var marketEnd = new TimeSpan(15, 30, 0);
                    var currentTime = DateTime.Now.TimeOfDay;
                    if (currentTime >= marketStart && currentTime <= marketEnd)
                    {
                        var getstockdata = await getstcoks();
                        var onlineUsers = StockNotificationHub.GetConnectedUsers();


                        foreach (var item in onlineUsers)
                        {
                            if (getstockdata != null && getstockdata.Any())
                            {
                                await _hubContext.Clients.User(item).SendAsync("ReceiveStockUpdate", getstockdata);
                            }
                        }

                    }


                    await Task.Delay(TimeSpan.FromMinutes(time), stoppingToken);
                }
                
            }
        }
        private List<StockData> GetLiveStockPrice(List<Stocks> symbol)
        {
            if (symbol != null)
            {
                List<StockData> stockData = _fyersWebSocketService.GetStockData(symbol);
                return stockData;
            }
            else
            {
                return new List<StockData>();
            }

        }

        private async Task<List<Stocknotifactiondata>> getstcoks()
        {
            List<Stocknotifactiondata> stocks = new List<Stocknotifactiondata>();
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
                    var assignedStocks = users.Stocks?.Where(x => x.StockNotification == true).ToList() ?? new List<Stocks>();
                    List<string?> userstrategy = users?.UserStrategy?.Where(x => x.StretagyEnableDisable == true).Select(x=>x.StretagyName).ToList();
                    if (assignedStocks != null && assignedStocks.Any())
                    {
                        users.UserStrategy = users.UserStrategy.Where(x => x.StretagyEnableDisable == true && x.IsActive == true).ToList();
                        var stockPrices = GetLiveStockPrice(assignedStocks);
                        foreach (var item in assignedStocks)
                        {
                            var signal = await getfinalsignal(item.Symbol, users.UserStrategy);
                            if (!string.IsNullOrEmpty(signal))
                            {
                                await userRepo.UpdateUserStocks(users?.Id.ToString(), item?.Symbol, signal);
                            }
                            stocks.Add(new Stocknotifactiondata()
                            {
                                BuySellSignal = signal,
                                Change = stockPrices.Where(x => x.Symbol == item.Symbol)?.FirstOrDefault()?.Change,
                                Price = stockPrices.Where(x => x.Symbol == item.Symbol)?.FirstOrDefault()?.Price,
                                Priviscloseprice = stockPrices.Where(x => x.Symbol == item.Symbol)?.FirstOrDefault()?.prev_close_price,
                                Symbol = item.Symbol,
                                userid = users?.Id.ToString(),
                                timestamp = DateTime.Now,
                                CompanyName=item.CompanyName,
                                userStrategy = userstrategy.Count() != 0 ? userstrategy : new List<string>()
                            });

                        }

                    }
                    return stocks;
                }
               
            }
            return stocks;
        }

        private async Task<AdminSettings> GetAdminsetting()
        {
            AdminSettings adminSettings = new AdminSettings();
            using (var scope = _serviceProvider.CreateScope())
            {
                var Admindsetting=scope.ServiceProvider.GetRequiredService<IUserRepository>();
                adminSettings = await Admindsetting.GetUserSettings();
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

    }
}
