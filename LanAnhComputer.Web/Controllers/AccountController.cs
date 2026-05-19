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

        // LOGIN
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

            // Lưu JWT
            HttpContext.Session.SetString("JWT", result.Token);

            // Lưu tên user nếu API có trả về
            HttpContext.Session.SetString("FullName", result.FullName ?? "User");

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
            var result = await _accountService.RegisterAsync(model);

            if (result == null)
            {
                return BadRequest(new
                {
                    message = "Đăng ký thất bại"
                });
            }

            HttpContext.Session.SetString("JWT", result.Token);

            HttpContext.Session.SetString("FullName", result.FullName ?? "User");

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
