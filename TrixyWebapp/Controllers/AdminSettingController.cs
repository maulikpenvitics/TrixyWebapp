using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Repository.IRepositories;
using Repository.Models;
using Repository.Repositories;
using System.Text;
using TrixyWebapp.Filters;

namespace TrixyWebapp.Controllers
{
    [Authorize(Roles = "Admin")]
    [SessionCheck]
    public class AdminSettingController : Controller
    {
        private readonly IRepository<AdminSettings> _adminSettingsRepository;
        private readonly IAdminSettingRepository _adminSettings;
        private readonly IErrorHandlingRepository _errorhandling;
        public AdminSettingController(IRepository<AdminSettings> adminSettingsRepository,
            IAdminSettingRepository adminSettings,
            IErrorHandlingRepository errorhandling)
        {
            _adminSettingsRepository = adminSettingsRepository;
            _adminSettings = adminSettings;
            _errorhandling = errorhandling;
        }

        public IActionResult Index()
        {

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AdminSetting()
        {
            try
            {
                var symbollist = _adminSettingsRepository.GetAllAsync().Result.FirstOrDefault();
                //var adminseting = _user.GetUserSettings("67bef9c5bc1d49323084998f");

                return View(symbollist);
            }
            catch (Exception ex)
            {
                await _errorhandling.AddErrorHandling(ex, "AdminSetting");
                return View(new AdminSettings());
            }
         
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
            catch(Exception ex)
            {
                await _errorhandling.AddErrorHandling(ex, "AdminSetting");
                ViewBag.ErrorMessage = "Invalid JSON format!";
                var symbollist = _adminSettingsRepository.GetAllAsync().Result.FirstOrDefault();
                return View(symbollist);
            }

            return View(model);
        }
    }
}
