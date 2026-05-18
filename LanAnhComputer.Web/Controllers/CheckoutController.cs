using LanAnhComputer.API.Dtos;
using LanAnhComputer.Web.Models;
using LanAnhComputer.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using LanAnhComputer.Web.Extensions;

namespace LanAnhComputer.Web.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly HttpClient _httpClient;

        public CheckoutController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var token = HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            _httpClient.AddJwt(token);

            var dto = new CheckoutDtos
            {
                ShippingFullName = model.FullName,
                ShippingPhone = model.Phone,
                ShippingAddressLine = model.Address,
                ShippingCity = model.Province,
                ShippingDistrict = model.District,
                ShippingWard = model.Ward,
                PaymentMethod = model.PaymentMethod,
                Note = model.Note,
                DiscountAmount = 0,
                ShippingFee = 0
            };

            var response = await _httpClient.PostAsJsonAsync(
                "https://localhost:7132/api/Orders/checkout",
                dto
            );

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Success");

            return View(model);
        }

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            _httpClient.AddJwt(token);

            // 1. GET DTO từ API
            var dtoList = await _httpClient.GetFromJsonAsync<List<CartItemDto>>(
                "https://localhost:7132/api/cart"
            );

            // 2. MAP DTO → VIEWMODEL (QUAN TRỌNG)
            var model = dtoList?.Select(x => new CartItemViewModel
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                Price = x.Price,
                Quantity = x.Quantity,
                ImageUrl = "" // nếu API chưa có thì để tạm
            }).ToList();
            ViewBag.SubTotal = model?.Sum(x => x.Price * x.Quantity) ?? 0;
            return View(model ?? new List<CartItemViewModel>());
        }
    }
}