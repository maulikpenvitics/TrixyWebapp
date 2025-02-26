using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Repository.IRepositories;
using Repository.Models;


namespace TrixyWebapp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IRepository<User> _userRepository;
        public UserController(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
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
            if (ModelState.IsValid) 
            {
                if (user.Id.ToString() !=null && user.Id != ObjectId.Empty && user.Id.ToString() != "000000000000000000000000")
                {
                    await _userRepository.UpdateAsync(user.Id.ToString(), user);
                    return RedirectToAction("Index");
                }
                else
                {
                    var existuser= await _userRepository.getUserByEmail(user?.Email??"");
                    if (existuser ==null)
                    {
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
            else
            {
                ViewBag.Errormessage = "Please try agin";
                return View();
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
      
    }
}
