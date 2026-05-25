using LanAnhComputer.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace LanAnhComputer.Web.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ICheckoutService _checkoutService;

        public OrdersController(ICheckoutService checkoutService)
        {
            _checkoutService = checkoutService;
        }

        [HttpGet]
        public async Task<IActionResult> CheckPaymentStatus(long orderId)
        {
            var token = HttpContext.Session.GetString("JWT") ?? "";

            var isPaid = await _checkoutService
                .CheckPaymentStatusAsync(orderId, token);

            return Json(new
            {
                success = isPaid
            });
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? status)
        {
            var token = HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
                return Redirect("/");

            var orders = await _checkoutService.GetMyOrdersAsync(token);

            if (!string.IsNullOrEmpty(status))
            {
                orders = orders
                    .Where(x => x.OrderStatus == status)
                    .ToList();
            }

            return View(orders);
        }
        [HttpPost]
        public async Task<IActionResult> CancelOrder(long orderId) //  Huỷ dơn hàng
        {
            var token = HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
                return Redirect("/");

            var success = await _checkoutService.CancelOrderAsync(orderId, token);

            if (!success)
                TempData["Error"] = "Hủy đơn thất bại";

            return RedirectToAction("Index");
        }
    }
}