using LanAnhComputer.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace LanAnhComputer.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly HttpClient _httpClient;

        public AdminController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

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
