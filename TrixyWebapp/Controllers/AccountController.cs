using Microsoft.AspNetCore.Mvc;
using Repository.IRepositories;
using Repository.Models;

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
            var getuser= await _userRepository.GetAllAsync();
           
            return View(getuser);
        }

        public IActionResult Login()
        {
            return View();
        }
    }
}
