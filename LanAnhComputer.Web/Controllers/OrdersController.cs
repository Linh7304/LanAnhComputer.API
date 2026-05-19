using Microsoft.AspNetCore.Mvc;

namespace LanAnhComputer.Web.Controllers
{
    public class OrdersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult CheckPaymentStatus(long orderId)
        {
            // demo fake

            return Json(new
            {
                success = true
            });
        }
    }
}