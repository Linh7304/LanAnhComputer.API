using Microsoft.AspNetCore.Mvc;

namespace LanAnhComputer.Web.Controllers
{
    public class CartController :Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
