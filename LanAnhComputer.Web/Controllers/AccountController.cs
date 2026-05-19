using LanAnhComputer.Web.Models;
using LanAnhComputer.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace LanAnhComputer.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
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
                : string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl;

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
                    message = "Đăng ký thất bại"
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

        private void SaveUserSession(AuthResponseViewModel result)
        {
            HttpContext.Session.SetString("JWT", result.Token);
            HttpContext.Session.SetString("FullName", result.FullName ?? "User");
            HttpContext.Session.SetString("Role", result.Role);
        }
    }
}
