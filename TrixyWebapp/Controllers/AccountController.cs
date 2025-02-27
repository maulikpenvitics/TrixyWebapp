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


namespace TrixyWebapp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IRepository<User> _userRepository;
        private readonly IUserRepository _user;

        public AccountController(IRepository<User> userRepository, IUserRepository user)
        {
            _userRepository = userRepository;
            _user = user;
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
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userRepository.AuthenticateUserAsync(model.Email, model.Password);

            if (user != null)
            {
                HttpContext.Session.Set("UserId", Encoding.UTF8.GetBytes(user.Id.ToString()));
                HttpContext.Session.SetString("UserRole", user?.Role ?? "");
                HttpContext.Session.SetString("UserName", user?.Firstname+ " "+ user?.Lastname);


                var claims = new List<Claim>
                      {
                            new Claim(ClaimTypes.Name, user.Email),
                             new Claim(ClaimTypes.Role, user.Role)
                       };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity), authProperties);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewData["LoginError"] = "Invalid email or password. Please try again.";
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
        public async Task<IActionResult> UserProfile(CreateUserViewModel model, IFormFile ProfileImage)
        {
            var existuser = await _userRepository.GetByIdAsync(model?.Id ?? "");
            if (ModelState.IsValid)
            {
                string? fileName = null;
                if (ProfileImage != null)
                {
                    string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Uploads");
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    fileName = Guid.NewGuid().ToString() + Path.GetExtension(ProfileImage.FileName);
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProfileImage.CopyToAsync(fileStream);
                    }

                    model.ProfileImageUrl = "/Uploads/" + fileName;
                }

                var user = new User
                {
                    Id = string.IsNullOrEmpty(model?.Id) ? ObjectId.GenerateNewId() : ObjectId.Parse(model.Id),
                    Firstname = model?.Firstname,
                    Lastname = model?.Lastname,
                    Email = model?.Email,
                    Password = model?.Password,
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
