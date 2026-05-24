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

        // Existing action
        [HttpGet]
        public async Task<IActionResult> CheckPaymentStatus(long orderId)
        {
            var token = HttpContext.Session.GetString("JWT") ?? "";
            var isPaid = await _checkoutService.CheckPaymentStatusAsync(orderId, token);
            return Json(new { success = isPaid });
        }

        // New Index action for order overview after COD
        [HttpGet]
        public IActionResult Index()
        {
            // TODO: fetch user's orders and pass to view
            // For now display a simple confirmation page.
            return View();
        }
    
    [HttpGet]
    public IActionResult Index()
    {
        // Placeholder: In production, fetch user's orders here.
        return View();
    }