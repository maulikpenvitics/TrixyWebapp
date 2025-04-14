﻿using Microsoft.AspNetCore.Authorization;
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
            if (user!=null && user.Id.ToString() != null && user.Id != ObjectId.Empty && user.Id.ToString() != "000000000000000000000000")
            {
                var existuser = await _user.GetByEmail(user?.Email ?? "");

               var aoivdstraetgy= existuser?.UserStrategy?.FirstOrDefault(x => x.StretagyName == "Sentiment_Analysis");
                if (aoivdstraetgy!=null)
                {
                    aoivdstraetgy.IsActive = false;
                    existuser?.UserStrategy?.Add(aoivdstraetgy);
                }
                user!.ProfileImageUrl = existuser?.ProfileImageUrl;   
                user.UserStrategy = existuser?.UserStrategy;   
                user.Status = existuser?.Status;   
                user.Stocks = existuser?.Stocks;   
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
                                var isactive = true;
                                if (item.Strategy== "Sentiment_Analysis")
                                {
                                    isactive = false;
                                }
                                strategyWeight.Add(new UserStrategy()
                                {
                                    IsActive = isactive,
                                    StretagyEnableDisable = false,
                                    StretagyName = item.Strategy
                                });

                            }
                        }
                       
                    }
                    user!.Stocks = new List<Stocks>();
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
        public IActionResult AdminSettings()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var adminseting= _user.GetUserSettings();
            
            return View(adminseting);
        }

        public IActionResult Setting()
        {
            var userId = HttpContext.Session.GetString("UserId");
            //string? userId = userIdBytes != null ? Encoding.UTF8.GetString(userIdBytes) : null;
            var user = userId!=null? _user.GetById(userId):new Repository.Models.User();
            return View(user);
        }
    }
}
