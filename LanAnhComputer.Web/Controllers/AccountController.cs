using LanAnhComputer.API.Dtos;
using LanAnhComputer.Web.Models;
using LanAnhComputer.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace LanAnhComputer.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AccountController(
            IAccountService accountService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _accountService = accountService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Login(
            [FromBody] LoginViewModel model,
            string? returnUrl = null)
        {
            var result = await _accountService.LoginAsync(model);

            if (result == null)
            {
                return BadRequest(new
                {
                    message = "Sai tài khoản hoặc mật khẩu"
                });
            }

            SaveUserSession(result);

            var redirectTo = result.Role == "Admin"
                ? "/admin"
                : string.IsNullOrEmpty(returnUrl)
                    ? "/"
                    : returnUrl;

            return Ok(new
            {
                redirectTo
            });
        }

        [HttpPost]
        public async Task<IActionResult> Register(
            [FromBody] RegisterViewModel model)
        {
            var result = await _accountService.RegisterAsync(model);

            if (result == null)
            {
                return BadRequest(new
                {
                    message = "Tài khoản mật khẩu đã tồn tại "
                });
            }

            SaveUserSession(result);

            return Ok(new
            {
                redirectTo = "/"
            });
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var token =
                HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction(
                    "Index",
                    "Home");
            }

            var profile =
                await _accountService.GetProfileAsync(token);

            if (profile == null)
            {
                return RedirectToAction(
                    "Index",
                    "Home");
            }

            var model = new ProfileViewModel
            {
                FullName = profile.FullName,
                Email = profile.Email,
                Phone = profile.Phone,
                Gender = profile.Gender,
                DateOfBirth = profile.DateOfBirth
            };

            return View(
                "~/Views/Profile/Index.cshtml",
                model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(
            ProfileViewModel model)
        {
            var token =
                HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction(
                    "Index",
                    "Home");
            }

            var success =
                await _accountService.UpdateProfileAsync(
                    model,
                    token);

            TempData["Success"] =
                success
                ? "Cap nhat thong tin thanh cong"
                : "Cap nhat that bai";

            if (success)
            {
                HttpContext.Session.SetString(
                    "FullName",
                    model.FullName);
            }

            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            ProfileViewModel model)
        {
            var token =
                HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction(
                    "Index",
                    "Home");
            }

            var success =
                await _accountService.ChangePasswordAsync(
                    model,
                    token);

            TempData["Success"] =
                success
                ? "Đổi mật khẩu thành công"
                : "Đổi mật khẩu thất bại";

            return RedirectToAction(nameof(Profile));
        }
        private void SaveUserSession(
            AuthResponseViewModel result)
        {
            HttpContext.Session.SetString(
                "JWT",
                result.Token);

            HttpContext.Session.SetString(
                "FullName",
                result.FullName ?? "User");

            HttpContext.Session.SetString(
                "Role",
                result.Role);
        }
    }
}