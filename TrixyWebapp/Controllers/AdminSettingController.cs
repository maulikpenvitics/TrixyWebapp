using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Repository.FyersWebSocketServices;
using Repository.FyersWebSocketServices.Jobs;
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
        private readonly FyersWebSocketService _fyersWebSocketService;
        public AdminSettingController(IRepository<AdminSettings> adminSettingsRepository,
            IAdminSettingRepository adminSettings,
            IErrorHandlingRepository errorhandling,
               FyersWebSocketService  fyersWebSocketService )
        {
            _adminSettingsRepository = adminSettingsRepository;
            _adminSettings = adminSettings;
            _errorhandling = errorhandling;
            _fyersWebSocketService=fyersWebSocketService;
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

        [HttpPost]
        public async Task<IActionResult> UpdateUserAuthcode(string authcode)
        {
            var result = await _fyersWebSocketService.GenerateandupdateToken(authcode);

            if (result)
            {
                var updateadminauthcode = await _adminSettings.UpdateUserAuthcode(authcode);
                await _fyersWebSocketService.Connect();
                return Json(new { message = "Auth code update successfully." });
            }
            else
            {
                return Json(new { error = "Please pass valid token" });
            }

        }
        [HttpGet]
        public IActionResult GenerateAuthCode()
        {
            string authUrl = _fyersWebSocketService.generateAuthCode();
            return Json(new { url = authUrl });
        }
    }
}
