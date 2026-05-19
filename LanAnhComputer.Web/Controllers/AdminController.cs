using Microsoft.AspNetCore.Mvc;

namespace LanAnhComputer.Web.Controllers
{
    [Route("admin")]
    public class AdminController : Controller
    {
        [HttpGet("")]
        [HttpGet("dashboard")]
        public IActionResult Index()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpGet("san-pham")]
        public IActionResult Products()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpGet("chatbot")]
        public IActionResult Chatbot()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }
    }
}
