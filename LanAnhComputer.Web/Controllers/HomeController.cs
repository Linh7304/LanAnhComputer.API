using LanAnhComputer.Web.Models;
using LanAnhComputer.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace LanAnhComputer.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;

        public HomeController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync(
                "https://localhost:7132/api/Products"
            );

            if (!response.IsSuccessStatusCode)
            {
                return View(new List<ProductViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PagedResult<ProductViewModel>>
(
    json,
    new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    }
);

            var products = result?.Items ?? new List<ProductViewModel>();


            return View(products);
        }
    }
}