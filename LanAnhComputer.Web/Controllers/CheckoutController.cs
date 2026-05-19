using LanAnhComputer.Web.Models;
using LanAnhComputer.Web.Services;
using Microsoft.AspNetCore.Mvc;
using LanAnhComputer.Web.ViewModels;

namespace LanAnhComputer.Web.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly ICheckoutService _checkoutService;

        public CheckoutController(
            ICartService cartService,
            ICheckoutService checkoutService)
        {
            _cartService = cartService;
            _checkoutService = checkoutService;
        }

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWT");

            // dùng modal => không redirect Login
            if (string.IsNullOrEmpty(token))
            {
                return Redirect("/");
            }

            var model = new CheckoutViewModel
            {
                CartItems = await _cartService.GetCartItemsAsync(token)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(
            CheckoutViewModel model)
        {
            var token = HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized();
            }

            var result = await _checkoutService
                .PlaceOrderAsync(model, token);

            if (result == null)
            {
                model.CartItems =
                    await _cartService.GetCartItemsAsync(token);

                ModelState.AddModelError(
                    string.Empty,
                    "Không thể đặt hàng"
                );

                return View("Index", model);
            }

            // COD
            if (model.PaymentMethod == "COD")
            {
                return RedirectToAction("Complete");
            }

            // BANKING / EWallet
            return RedirectToAction("Payment", new
            {
                orderId = result.OrderId
            });
        }

        public async Task<IActionResult> Payment(long orderId)
        {
            var token = HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
            {
                return Redirect("/");
            }

            var order = new OrderViewModel
            {
                OrderId = orderId,

                TotalAmount = 15000000
            };

            return View(order);
        }

        public IActionResult Complete()
        {
            return View();
        }
    }
}