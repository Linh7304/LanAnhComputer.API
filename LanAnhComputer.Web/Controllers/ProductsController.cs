using Microsoft.AspNetCore.Mvc;

namespace LanAnhComputer.Web.Controllers
{
    public class ProductsController :Controller
    {
        public IActionResult Index(string category, string brand, string price)
        {
            // Logic lọc sản phẩm
            return View();
        }

    }
}
