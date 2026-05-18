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

        // LOGIN
        [HttpPost]
        public async Task<IActionResult> Login(
            [FromBody] LoginViewModel model,
            string? returnUrl = null)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "https://localhost:7132/api/Auth/login",
                model);

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest(new
                {
                    message = "Sai tài khoản hoặc mật khẩu"
                });
            }

            var result = await response.Content
                .ReadFromJsonAsync<AuthResponseViewModel>();

            // Lưu JWT
            HttpContext.Session.SetString("JWT", result!.Token);

            // Lưu tên user nếu API có trả về
            HttpContext.Session.SetString("FullName",result.FullName ?? "User");

            return Ok(new
            {
                redirectTo = string.IsNullOrEmpty(returnUrl)
                    ? "/"
                    : returnUrl
            });
        }

        // REGISTER
        [HttpPost]
        public async Task<IActionResult> Register(
            [FromBody] RegisterViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "https://localhost:7132/api/Auth/register",
                model);

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest(new
                {
                    message = "Đăng ký thất bại"
                });
            }

            var result = await response.Content
                .ReadFromJsonAsync<AuthResponseViewModel>();

            HttpContext.Session.SetString("JWT", result!.Token);

            HttpContext.Session.SetString("FullName",result.FullName ?? "User");

            return Ok(new
            {
                redirectTo = "/"
            });
        }

        // LOGOUT
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }
    }
}