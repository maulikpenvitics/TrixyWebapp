﻿using FyersCSharpSDK;
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
using NuGet.ProjectModel;
using TrixyWebapp.Filters;


namespace TrixyWebapp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IRepository<User> _userRepository;
        private readonly IUserRepository _user;
        private readonly IWebHostEnvironment _env;
        private readonly FyersWebSocketService _fyersWebSocket;
        private readonly IErrorHandlingRepository  _errorhandling;
        public AccountController(IRepository<User> userRepository, IUserRepository user, IWebHostEnvironment env,
            FyersWebSocketService fyersWebSocket, IErrorHandlingRepository errorhandling)
        {
            _userRepository = userRepository;
            _user = user;
            _env = env;
            _fyersWebSocket = fyersWebSocket;
            _errorhandling = errorhandling;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<User>getuser = new List<User>();
            try
            {
                 getuser = await _userRepository.GetAllAsync();
                return View(getuser);
            }
            catch (Exception ex)
            {
                await _errorhandling.AddErrorHandling(ex,"Account/Index");
                return View(getuser);
            }
           
           
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

                var user = await _userRepository.AuthenticateUserAsync(model?.Email??"", model?.Password ?? "");

                if (user != null)
                {
                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("UserRole", user?.Role ?? "");
                    HttpContext.Session.SetString("UserName", user?.Firstname + " " + user?.Lastname);
                    HttpContext.Session.SetString("imageurl", user?.ProfileImageUrl ?? "");
                    HttpContext.Session.SetString("User", JsonSerializer.Serialize(user));

                    var userJson = HttpContext?.Session.GetString("User");
                    var userId = HttpContext?.Session.GetString("UserId");
                    Helper.LogFile(userJson??"",_env);
                    Helper.LogFile(userId??"",_env);
                    var claims = new List<Claim>
                    {
                              new Claim(ClaimTypes.NameIdentifier, userId??""), // ✅ Add User ID
                               new Claim(ClaimTypes.Name, user?.Firstname??""),
                              new Claim(ClaimTypes.Role, user ?.Role ?? "")
                    };


                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties { IsPersistent = true };

                    await HttpContext!.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity), authProperties);
                   
                    //await _fyersWebSocket.Connect();
                    Helper.LogFile(userId ?? "", _env);
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
                await _errorhandling.AddErrorHandling(ex, "Login");
                Helper.LogFilegenerate(ex, "Login Action", _env);
                return View(model);
            }
          
        }

        public async Task<IActionResult> Logout()
        {
            try
            {
                _fyersWebSocket.Disconnect();
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.Session.Clear();

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                await _errorhandling.AddErrorHandling(ex, "Account/LogOut");
                return RedirectToAction("Login", "Account");
            }
          
        }

        
        [HttpGet]
        public async Task<IActionResult> ChangePassword(string Id)
        {
            try
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
            catch (Exception ex)
            {
                await _errorhandling.AddErrorHandling(ex, "Account/ChangePassword");
                return RedirectToAction("Index", "Home");
            }
        
        
        }

        
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePassword changePassword)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(changePassword?.Id ?? "");
                if (changePassword != null && !string.IsNullOrEmpty(changePassword.Id))
                {
                    var result = await _user.ChangePassword(changePassword?.Id ?? "", changePassword?.Oldpassword ?? "", changePassword?.NewPassword ?? "");
                    if (result == 1)
                    {
                        return RedirectToAction("Index", "Home");
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
            catch (Exception ex)
            {
                await _errorhandling.AddErrorHandling(ex, "Account/ChangePassword");
                return RedirectToAction("Index", "Home");
            }
          
        }

      
        [HttpGet]
        public async Task<IActionResult> UserProfile(string Id)
        {
            try
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
            catch (Exception ex)
            {
                await _errorhandling.AddErrorHandling(ex, "Account/UserProfile");
                return RedirectToAction("Index", "Home");
            }
           
        }
        
        [HttpPost]
        public async Task<IActionResult> UserProfile(CreateUserViewModel model)
        {
            try
            {
                var existuser = await _userRepository.GetByIdAsync(model?.Id ?? "");
                if (ModelState.IsValid)
                {
                    string? fileName = null;
                    if (model?.ProfileImage != null)
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
                        ProfileImageUrl = model?.ProfileImageUrl??null// Save path in DB
                    };
                    var result = await _user.UpdateUserProfile(user);
                    HttpContext.Session.SetString("imageurl", user?.ProfileImageUrl ?? "");
                    HttpContext.Session.SetString("UserName", user?.Firstname + " " + user?.Lastname);
                    if (result)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return View(existuser);
                    }
                }

                return View(existuser);
            }
            catch (Exception ex)
            {
                await _errorhandling.AddErrorHandling(ex, "Account/UserProfile");
                return RedirectToAction("Index", "Home");
            }
            
        }

    }
}
