using LanAnhComputer.Web.Models;
using LanAnhComputer.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace LanAnhComputer.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/Cart" });
            }

            var model = await _cartService.GetCartItemsAsync(token);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequestViewModel request)
        {
            var token = HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized();
            }

            var isSuccess = await _cartService.AddToCartAsync(request, token);

            if (!isSuccess)
            {
                return BadRequest();
            }

            return Ok();
        }
    }
}
