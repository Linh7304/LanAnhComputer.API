using LanAnhComputer.API.Dtos;
using LanAnhComputer.Web.Extensions;
using LanAnhComputer.Web.Models;
using LanAnhComputer.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

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
            _httpClient.AddJwt(token);
            // 1. LẤY DATA TỪ API (DTO)

            var dtoList = await _httpClient.GetFromJsonAsync<List<CartItemDto>>("https://localhost:7132/api/cart");

            // 2. MAP DTO → VIEWMODEL (ĐẶT NGAY SAU ĐOẠN TRÊN)
            var model = dtoList?.Select(x => new CartItemViewModel
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                Price = x.Price,
                Quantity = x.Quantity,
                ImageUrl = x.ImageUrl,
                Brand = x.Brand,
                ProductType = x.ProductType
            }).ToList();

            // 3. RETURN VIEW
            return View(model ?? new List<CartItemViewModel>());
        }
         
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequestViewModel request)
        {
            var token = HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized();
            }

            _httpClient.AddJwt(token);

            var response = await _httpClient.PostAsJsonAsync("https://localhost:7132/api/cart/add",request);

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest();
            }

            return Ok();
        }

    }
}