using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Repository.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.FyersWebSocketServices.Jobs
{
    public class AuthSchedulerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly FyersWebSocketService _fyersWebSocketService;
        private readonly FyersStockSettings _settings;
        public AuthSchedulerService(IServiceProvider serviceProvider, FyersWebSocketService fyersWebSocketService, IOptions<FyersStockSettings> settings)
        {
            _serviceProvider = serviceProvider;
            _fyersWebSocketService = fyersWebSocketService;
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            BackgroundJob.Schedule(() => Refreshtokengenerate(), TimeSpan.FromDays(13));
            await Task.CompletedTask;
        }

        public async Task Refreshtokengenerate()
        {
            using (var scope = _serviceProvider.CreateScope()) {
              var UserRepo= scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var adminsetting = await UserRepo.GetUserSettings();
                var authdata=adminsetting.UserAuthtoken;

                if (authdata != null) 
                {
                    var validatetoken =await _fyersWebSocketService.ValidateToken(authdata?.access_token??"");
                    if (!validatetoken)
                    {
                        var getrefreshtoken = await _fyersWebSocketService.GetRefreshToken(authdata?.refresh_token??"", authdata?.Pin??"");
                        if (!string.IsNullOrEmpty(getrefreshtoken?.access_token))
                        {
                            var updatetoken = await UserRepo.UpdateAdminAuthtoken(getrefreshtoken?.access_token??"");
                            if (updatetoken !=null)
                            {
                                _settings!.AccessToken = updatetoken.access_token;
                                Console.WriteLine("Refresh token successfully");
                            }
                            else {
                                Console.WriteLine("Please try agin");
                            }
                        }
                       
                    }
                
                }

            }
        }
    }
}
