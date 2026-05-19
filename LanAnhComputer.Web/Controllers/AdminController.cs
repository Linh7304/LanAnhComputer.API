using Microsoft.AspNetCore.Mvc;

namespace LanAnhComputer.Web.Controllers
{
    public class AdminController : Controller
    {
        [Route("admin")]
        [Route("dashboard")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("san-pham")]
        public IActionResult Products()
        {
            return View();
        }

        [Route("chatbot")]
        public IActionResult Chatbot()
        {
            return View();
        }
    }
}
