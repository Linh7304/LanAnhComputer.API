using LanAnhComputer.Web.Services;
using LanAnhComputer.Constants;
using Microsoft.AspNetCore.Mvc;

namespace LanAnhComputer.Web.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ICheckoutService _checkoutService;
        private readonly IOrderService _orderService;

        public OrdersController(ICheckoutService checkoutService, IOrderService orderService)
        {
            _checkoutService = checkoutService;
            _orderService = orderService;
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
                    .Where(x => string.Equals(x.OrderStatus, status, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return View(orders);
        }

        [HttpGet("Orders/Details")]
        public async Task<IActionResult> Details(long orderId)
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return Redirect("/");

            var order = await _orderService.GetOrderByIdAsync(orderId, token);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
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
