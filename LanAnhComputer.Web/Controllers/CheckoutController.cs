using LanAnhComputer.Data.Entities;
using LanAnhComputer.Web.Models;
using LanAnhComputer.Web.Services;
using LanAnhComputer.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            Console.WriteLine("=== PLACE ORDER ===");
            var token = HttpContext.Session.GetString("JWT");

            Console.WriteLine($"TOKEN: {token}");

            Console.WriteLine($"ModelState valid: {ModelState.IsValid}");
foreach (var item in ModelState)
            {
                Console.WriteLine($"KEY: {item.Key}");

                foreach (var error in item.Value.Errors)
                {
                    Console.WriteLine($"ERROR: {error.ErrorMessage}");
                }
            }
            if (string.IsNullOrEmpty(token))
                return Redirect("/");

            if (!ModelState.IsValid)
            {
                model.CartItems = await _cartService.GetCartItemsAsync(token);
                return View("Index", model);
            }

            var result = await _checkoutService.PlaceOrderAsync(model, token);

            if (result == null)
            {
                model.CartItems = await _cartService.GetCartItemsAsync(token);
                ModelState.AddModelError("", "Không thể đặt hàng");
                return View("Index", model);
            }

            // =========================
            // 1. COD
            // =========================
            if (string.Equals(model.PaymentMethod, "COD", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index","Orders");
            }

            // =========================
            // 2. BANK / E-WALLET
            // =========================
            return RedirectToAction(
              "Payment",
              new
              {
                  orderId = result.OrderId
              }
          );
        }

        public async Task<IActionResult> Payment(long orderId)
        {
            var token = HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
            {
                return Redirect("/");
            }

            var payment = await _checkoutService.CreatePaymentAsync(orderId, token);

            if (payment == null)
            {
                return RedirectToAction("Index");
            }

            return View(payment);
        }

        public IActionResult Complete(long orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }
    }
}
