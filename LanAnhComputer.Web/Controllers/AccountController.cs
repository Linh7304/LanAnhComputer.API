using LanAnhComputer.Web.Models;
using LanAnhComputer.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace LanAnhComputer.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;

        public AccountController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "https://localhost:7132/api/Auth/login",
                model);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
                return View(model);
            }

            var result = await response.Content
                .ReadFromJsonAsync<AuthResponseViewModel>();

            // Lưu JWT vào Session
            HttpContext.Session.SetString("JWT", result!.Token);

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}