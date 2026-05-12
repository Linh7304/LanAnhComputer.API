using LanAnhComputer.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

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
            var model = await _httpClient
                .GetFromJsonAsync<List<CartItemViewModel>>("https://localhost:7132/api/cart{userId}");

            return View(model ?? new List<CartItemViewModel>());
        }
    }
}