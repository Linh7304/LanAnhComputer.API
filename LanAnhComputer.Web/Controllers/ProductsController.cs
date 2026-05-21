using LanAnhComputer.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace LanAnhComputer.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, string? sort = null)
        {
            int pageSize = 8; // 4 hàng tùy layout

            var response = await _productService.GetProductsAsync(pageNumber, pageSize, sort);

            return View(response);
        }

        public async Task<IActionResult> Details(long id)
        {
            var product = await _productService.GetProductDetailsAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(long productId, int rating, string? comment)
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Details", new { id = productId });
            }

            await _productService.SubmitReviewAsync(productId, rating, comment, token);

            return RedirectToAction("Details", new { id = productId });
        }
    }
}
