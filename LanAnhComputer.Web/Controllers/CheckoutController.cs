using LanAnhComputer.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace LanAnhComputer.Web.Controllers
{
    public class CheckoutController: Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult PlaceOrder(CheckoutViewModel model)
        {
            // Xử lý đặt hàng
            return RedirectToAction("Success");
        }
    }
}
