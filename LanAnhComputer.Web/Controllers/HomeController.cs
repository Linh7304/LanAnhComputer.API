using LanAnhComputer.Dtos;
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
            
            // Deserialize PagedResultDto<ProductDto> structure
            var pagedResult = JsonSerializer.Deserialize<PagedResultDto<ProductDto>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );

            // Map ProductDto to ProductViewModel
            var products = pagedResult?.Items?.Select(dto => new ProductViewModel
            {
                ProductId = dto.ProductId,
                ProductName = dto.ProductName,
                SalePrice = dto.SalePrice,
                OriginalPrice = dto.CostPrice,
                Rating = 0, // Default value
                Reviews = 0, // Default value
                Badge = dto.IsActive ? "Hot" : null,
                ImageUrl = dto.ImageUrl,
                Brand = dto.Brand
            }).ToList() ?? new List<ProductViewModel>();

            return View(products);
        }
    }
}