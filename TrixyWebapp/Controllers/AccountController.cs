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

namespace TrixyWebapp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IRepository<User> _userRepository;

        public AccountController(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
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
    }
}
