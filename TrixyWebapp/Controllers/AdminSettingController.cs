using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Repository.IRepositories;
using Repository.Models;
using Repository.Repositories;
using System.Text;

namespace TrixyWebapp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminSettingController : Controller
    {
        private readonly IRepository<AdminSettings> _adminSettingsRepository;
        private readonly IAdminSettingRepository _adminSettings;

        public AdminSettingController(IRepository<AdminSettings> adminSettingsRepository, IAdminSettingRepository adminSettings)
        {
            _adminSettingsRepository = adminSettingsRepository;
            _adminSettings = adminSettings;
        }

        public IActionResult Index()
        {

            return View();
        }

        [HttpGet]
        public  IActionResult AdminSetting()
        {
            var symbollist =  _adminSettingsRepository.GetAllAsync().Result.FirstOrDefault();
            //var adminseting = _user.GetUserSettings("67bef9c5bc1d49323084998f");

            return View(symbollist);
        }

        [HttpPost]
        public async Task<IActionResult> AdminSetting(string FullModelJson)
        {
            AdminSettings? model;
            try
            {
                model = JsonConvert.DeserializeObject<AdminSettings>(FullModelJson);
                await _adminSettingsRepository.UpdateAsync(model?.Id ?? "".ToString(), model??new AdminSettings());
                ViewBag.SuccessMessage = "Admin settings saved successfully!";
            }
            catch
            {
               
                ViewBag.ErrorMessage = "Invalid JSON format!";
                var symbollist = _adminSettingsRepository.GetAllAsync().Result.FirstOrDefault();
                return View(symbollist);
            }

            return View(model);
        }
    }
}
