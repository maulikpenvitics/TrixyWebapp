using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Repository.IRepositories;
using Repository.Models;
using System.Text;
using System.Text.Json;
using TrixyWebapp.Filters;


namespace TrixyWebapp.Controllers
{
    [Authorize]
    [SessionCheck]
    public class UserController : Controller
    {
        private readonly IRepository<User> _userRepository;
        private readonly IUserRepository _user;
        private readonly IErrorHandlingRepository _errorhandling;
        public UserController(IRepository<User> userRepository , IUserRepository user, IErrorHandlingRepository errorhandling)
        {
            _userRepository = userRepository;
            _user = user;
            _errorhandling = errorhandling;
        }
        public async Task<IActionResult> Index()
        {
            var userslist= await _userRepository.GetAllAsync();
            return View(userslist);
        }
       
        [HttpGet]
        public async Task<IActionResult> CreateUser(string? Id)
        {
            try
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
            catch (Exception ex)
            {
                await _errorhandling.AddErrorHandling(ex, "UserController/CreateUser");
                return View(new User());
            }
         
        }
        [HttpPost]
        public async Task<IActionResult> CreateUser(User user)
        {
            try
            {
                var adminsetting = await _user.GetUserSettings();
                var existuser = await _user.GetByEmail(user?.Email ?? "");
                if (user==null)
                {
                    ViewBag.Errormessage = "Invalid user data.";
                    return View("Index");
                }
                bool isUpdate = user.Id != ObjectId.Empty && user.Id.ToString() != "000000000000000000000000";

                if (isUpdate && existuser != null)
                {
                   
                    var sentimentStrategy = existuser?.UserStrategy?.FirstOrDefault(x => x.StretagyName == "Sentiment_Analysis");
                    if (sentimentStrategy != null)
                    {
                        sentimentStrategy.IsActive = false;
                        existuser?.UserStrategy?.Add(sentimentStrategy);
                    }
                    if (existuser?.UserStrategy==null && !existuser!.UserStrategy.Any())
                    {
                        existuser.UserStrategy = GenerateStrategies(adminsetting);
                    }
                    user!.ProfileImageUrl = existuser?.ProfileImageUrl;
                    user.UserStrategy = existuser!.UserStrategy;
                    user.Status = true;
                    user.Stocks = existuser?.Stocks??new List<Stocks>();
                    await _userRepository.UpdateAsync(user.Id.ToString(), user);
                    var currentuserid = HttpContext?.Session.GetString("UserId");
                    if (currentuserid == existuser.Id.ToString())
                    {
                        existuser = await _user.GetByEmail(user?.Email ?? "");
                        HttpContext!.Session.SetString("UserId", existuser.Id.ToString());
                        HttpContext.Session.SetString("UserRole", existuser?.Role ?? "");
                        HttpContext.Session.SetString("UserName", existuser?.Firstname + " " + existuser?.Lastname);
                        HttpContext.Session.SetString("imageurl", user?.ProfileImageUrl ?? "");
                        HttpContext.Session.SetString("User", JsonSerializer.Serialize(existuser));

                    }
                    TempData["message"] = "User updated successfully";
                    return RedirectToAction("Index");
                }
                else
                {
                   
                    if (existuser == null)
                    {
                        if (adminsetting != null)
                        {
                            user.UserStrategy = GenerateStrategies(adminsetting);
                        }
                        user!.Stocks = new List<Stocks>();
                        var result = await _userRepository.InsertAsync(user);
                        if (result == 1)
                        {
                            ViewBag.message = "User created succesfully";
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
            catch (Exception ex)
            {
                await _errorhandling.AddErrorHandling(ex, "UserController/CreateUser");
                ViewBag.Errormessage = "Please try agin";
                return View();
                
            }
           
        }
        [HttpPost]
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
        public async Task<IActionResult> AdminSettings()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                var adminseting = _user.GetUserSettings();

                return View(adminseting);
            }
            catch (Exception ex)
            {
                await _errorhandling.AddErrorHandling(ex, "UserController/AdminSettings");
                return View(new AdminSettings());
            }
           
        }

        public async Task<IActionResult> Setting()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                //string? userId = userIdBytes != null ? Encoding.UTF8.GetString(userIdBytes) : null;
                var user = userId != null ? _user.GetById(userId) : new Repository.Models.User();
                return View(user);
            }
            catch (Exception ex)
            {
                await _errorhandling.AddErrorHandling(ex, "UserController/Setting");
                return View(new User());
            }
        
        }

        private List<UserStrategy> GenerateStrategies(AdminSettings? adminSetting)
        {
            var strategies = new List<UserStrategy>();
            var activeStrategies = adminSetting?.StrategyWeighted?.Where(x => x.IsActive == true).ToList();

            if (activeStrategies != null)
            {
                foreach (var item in activeStrategies)
                {
                    strategies.Add(new UserStrategy
                    {
                        StretagyName = item.Strategy,
                        IsActive = item.Strategy != "Sentiment_Analysis",
                        StretagyEnableDisable = false
                    });
                }
            }

            return strategies;
        }
    }
}
