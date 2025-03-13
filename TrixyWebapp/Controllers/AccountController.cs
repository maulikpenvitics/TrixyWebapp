using FyersCSharpSDK;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.IRepositories;
using Repository.Models;
using Repository.Repositories;
using Repository.ViewModels;
using System.Security.Claims;
using System.Text;
using TrixyWebapp.ViewModels;
using MongoDB.Bson;
using TrixyWebapp.Helpers;
using System.Text.Json;
using Repository.FyersWebSocketServices;


namespace TrixyWebapp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IRepository<User> _userRepository;
        private readonly IUserRepository _user;
        private readonly IWebHostEnvironment _env;
        private readonly FyersWebSocketService _fyersWebSocket;
        public AccountController(IRepository<User> userRepository, IUserRepository user, IWebHostEnvironment env,
            FyersWebSocketService fyersWebSocket)
        {
            _userRepository = userRepository;
            _user = user;
            _env = env;
            _fyersWebSocket = fyersWebSocket;
        }

        public async Task<IActionResult> Index()
        {
            var getuser = await _userRepository.GetAllAsync();

            return View(getuser);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = await _userRepository.AuthenticateUserAsync(model.Email, model.Password);

                if (user != null)
                {
                    HttpContext.Session.Set("UserId", Encoding.UTF8.GetBytes(user.Id.ToString()));
                    HttpContext.Session.SetString("UserRole", user?.Role ?? "");
                    HttpContext.Session.SetString("UserName", user?.Firstname + " " + user?.Lastname);
                    HttpContext.Session.SetString("imageurl", user?.ProfileImageUrl ?? "");
                    HttpContext.Session.SetString("User", JsonSerializer.Serialize(user));

                    var claims = new List<Claim>
                    {
                              new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // ✅ Add User ID
                               new Claim(ClaimTypes.Name, user.Email),
                              new Claim(ClaimTypes.Role, user.Role)
                    };


                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties { IsPersistent = true };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity), authProperties);
                    _fyersWebSocket.Connect(user?.Stocks?.ToList());
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewData["LoginError"] = "Invalid email or password. Please try again.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Helper.LogFilegenerate(ex, "Login Action", _env);
                return View(model);
            }
          
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
        [HttpGet]
        public async Task<IActionResult> ChangePassword(string Id)
        {
            var user=await _userRepository.GetByIdAsync(Id);
            if (user != null)
            {
                return View(user);
            }
            else
            {
                return RedirectToAction("Index","Home");
            }
        
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePassword changePassword)
        {
            var user = await _userRepository.GetByIdAsync(changePassword?.Id??"");
            if (changePassword !=null && !string.IsNullOrEmpty(changePassword.Id))
            {
               var result = await _user.ChangePassword(changePassword?.Id??"",changePassword?.Oldpassword??"",changePassword?.NewPassword??"");
                if (result == 1)
                {
                    return RedirectToAction("Index","Home");
                }
                else
                {
                    ViewBag.errormessage = "Something went wrong. Please try again.";
                    return View(user);
                    
                }
            }
            else
            {
                ViewBag.errormessage = "Something went wrong. Please try again.";
                return View(user);
            }
          
        }
        [HttpGet]
        public async Task<IActionResult> UserProfile(string Id)
        {
            var user = await _userRepository.GetByIdAsync(Id);
            if (user != null)
            {
                return View(user);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UserProfile(CreateUserViewModel model)
        {
            var existuser = await _userRepository.GetByIdAsync(model?.Id ?? "");
            if (ModelState.IsValid)
            {
                string? fileName = null;
                if (model.ProfileImage != null)
                {
                    string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Uploads");
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfileImage.FileName);
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfileImage.CopyToAsync(fileStream);
                    }

                    model.ProfileImageUrl = "/Uploads/" + fileName;
                }

                var user = new User
                {
                    Id = string.IsNullOrEmpty(model?.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(model.Id),
                    Firstname = model?.Firstname,
                    Lastname = model?.Lastname,
                    Email = model?.Email,
                    ProfileImageUrl = model?.ProfileImageUrl // Save path in DB
                };
                var result=  await _user.UpdateUserProfile(user);
                if (result)
                {
                    return RedirectToAction("Index","Home");
                }
                else
                {
                    return View(existuser);
                }
            }

            return View(existuser);
        }

    }
}
