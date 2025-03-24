using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Repository.IRepositories;
using Repository.Models;
using System.Text;


namespace TrixyWebapp.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IRepository<User> _userRepository;
        private readonly IUserRepository _user;
        public UserController(IRepository<User> userRepository , IUserRepository user)
        {
            _userRepository = userRepository;
            _user = user;
        }
        public async Task<IActionResult> Index()
        {
            var userslist= await _userRepository.GetAllAsync();
            return View(userslist);
        }
       
        [HttpGet]
        public async Task<IActionResult> CreateUser(string? Id)
        {
            if (!string.IsNullOrEmpty(Id))
            {
                var user = await _userRepository.GetByIdAsync(Id);
                if (user != null)
                {
                    return View(user); // Pass existing user data for editing
                }
            }
            return View(new User());
        }
        [HttpPost]
        public async Task<IActionResult> CreateUser(User user)
        {
            if (user.Id.ToString() != null && user.Id != ObjectId.Empty && user.Id.ToString() != "000000000000000000000000")
            {
                var existuser = await _user.GetByEmail(user?.Email ?? "");
                user.ProfileImageUrl = existuser.ProfileImageUrl;   
                user.UserStrategy = existuser.UserStrategy;   
                user.Status = existuser.Status;   
                user.Stocks = existuser.Stocks;   
                await _userRepository.UpdateAsync(user.Id.ToString(), user);
                return RedirectToAction("Index");
            }
            else
            {
                var existuser = await _user.GetByEmail(user?.Email ?? "");
                if (existuser == null)
                {
                    List<UserStrategy> strategyWeight = new List<UserStrategy>();
                    var adminsetting =await _user.GetUserSettings();
                    if (adminsetting != null)
                    {
                      var stretagy= adminsetting?.StrategyWeighted?.Where(x => x.IsActive == true).ToList();
                        if (stretagy!=null && stretagy.Any())
                        {
                            foreach (var item in stretagy)
                            {
                                strategyWeight.Add(new UserStrategy()
                                {
                                    IsActive = true,
                                    StretagyEnableDisable = false,
                                    StretagyName = item.Strategy
                                });

                            }
                        }
                       
                    }
                    user.Stocks = new List<Stocks>();
                    user.UserStrategy = strategyWeight;
                    var result = await _userRepository.InsertAsync(user);
                    if (result == 1)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ViewBag.Errormessage = "Please try agin";
                        return View();
                    }
                }
                else
                {
                    ViewBag.Errormessage = "This user already exists.";
                    return View();
                }
            }

        }
        [HttpGet]
        public async Task<IActionResult> DeleteUser(string Id)
        {
            var user= await _userRepository.GetByIdAsync(Id);
            if (user != null)
            {
                user.Status=false;
                await _userRepository.UpdateAsync(user.Id.ToString(), user);
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Index");
            }
          
        }

        [HttpGet]
        public async Task<IActionResult> UserSetting(AdminSettings userSettings)
        {
            userSettings = new AdminSettings
            {
                UserId = "123456",
                NotificationStatus = true,
                Threshold = 50,
                MovingAverage = new MovingAverageSettings { SMA_Periods = 10, LMA_Periods = 50 },
                RSIThresholds = new RSIThresholds { Overbought = 70, Oversold = 30 },
                MACD_Settings = new MACDSettings { ShortEmaPeriod = 12, LongEmaPeriod = 26, SignalPeriod = 9 },
                StrategyWeighted = new List<StrategyWeight>
            {
                new StrategyWeight { Strategy = "Moving_Average", Weight = 10.8, IsActive = true },
                new StrategyWeight { Strategy = "RSI", Weight = 30, IsActive = true },
                new StrategyWeight { Strategy = "Bollinger_Bands", Weight = 14.3, IsActive = true },
                new StrategyWeight { Strategy = "Mean_Reversion", Weight = 14.3, IsActive = true },
                new StrategyWeight { Strategy = "VWAP", Weight = 15.3, IsActive = true },
                new StrategyWeight { Strategy = "MACD", Weight = 15.3, IsActive = true },
                new StrategyWeight { Strategy = "Sentiment_Analysis", Weight = 0, IsActive = true },
                new StrategyWeight { Strategy = "Combine_Strategy", Weight = 0, IsActive = true }
            }
            };
            await _user.InsertUserseting(userSettings);
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> AdminSettings()
        {
            var userIdBytes = HttpContext.Session.Get("UserId");
            string userId = userIdBytes != null ? Encoding.UTF8.GetString(userIdBytes) : null;
            var adminseting= _user.GetUserSettings();
            
            return View(adminseting);
        }

        public IActionResult Setting()
        {
            var userIdBytes = HttpContext.Session.Get("UserId");
            string? userId = userIdBytes != null ? Encoding.UTF8.GetString(userIdBytes) : null;
            var user = _user.GetById(userId);
            return View(user);
        }
    }
}
