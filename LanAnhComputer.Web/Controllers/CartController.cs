using LanAnhComputer.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;

namespace LanAnhComputer.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly HttpClient _httpClient;

        public CartController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction( "Login", "Account", new { returnUrl = "/Cart" }
 );
            }

            // Gắn token vào Header
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Gọi API
            var model = await _httpClient
                .GetFromJsonAsync<List<CartItemViewModel>>(
                    "https://localhost:7132/api/cart");

            return View(model ?? new List<CartItemViewModel>());
        }
    }
}