using LanAnhComputer.Dtos;
using LanAnhComputer.Web.Models;
using LanAnhComputer.Web.Services;
using LanAnhComputer.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LanAnhComputer.Web.Controllers
{
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IAdminInventoryService _inventoryService;
        private readonly IUserService _userService;

        public AdminController(
            IProductService productService,
            IOrderService orderService,
            IAdminInventoryService inventoryService,
            IUserService userService)
        {
            _productService = productService;
            _orderService = orderService;
            _inventoryService = inventoryService;
            _userService = userService;
        }

        [HttpGet("")]
        [HttpGet("dashboard")]
        public async Task<IActionResult> Index()
        {
            var token = GetAdminToken();
            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var summary = await _inventoryService.GetSummaryAsync(token);

            return View(new AdminDashboardViewModel
            {
                OutOfStockCount = summary?.OutOfStockCount ?? 0,
                LowStockCount = summary?.LowStockCount ?? 0,
                TopSellingProducts = summary?.TopSellingProducts ?? new List<TopSellingProductDto>()
            });
        }

        [HttpGet("san-pham")]
        public async Task<IActionResult> Products(int pageNumber = 1, string? search = null)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            var products = await _productService.GetProductsAsync(pageNumber, 50, null, search);
            ViewBag.Search = search;

            return View(products);
        }

        [HttpPost("san-pham/them")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(AdminProductFormViewModel productForm)
        {
            var token = GetAdminToken();
            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var isSuccess = await _productService.CreateProductAsync(productForm, token);
            TempData[isSuccess ? "Success" : "Error"] = isSuccess
                ? "Da them san pham."
                : "Khong the them san pham.";

            return RedirectToAction(nameof(Products));
        }

        [HttpPost("san-pham/sua")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProduct(AdminProductFormViewModel productForm)
        {
            var token = GetAdminToken();
            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var isSuccess = await _productService.UpdateProductAsync(productForm, token);
            TempData[isSuccess ? "Success" : "Error"] = isSuccess
                ? "Da cap nhat san pham."
                : "Khong the cap nhat san pham.";

            return RedirectToAction(nameof(Products));
        }

        [HttpPost("san-pham/xoa/{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(long id)
        {
            var token = GetAdminToken();
            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var isSuccess = await _productService.DeleteProductAsync(id, token);
            TempData[isSuccess ? "Success" : "Error"] = isSuccess
                ? "Da xoa san pham."
                : "Khong the xoa san pham.";

            return RedirectToAction(nameof(Products));
        }

        [HttpGet("san-pham/{id:long}/chi-tiet")]
        public async Task<IActionResult> ProductDetails(long id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            var product = await _productService.GetProductDetailsAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost("san-pham/{id:long}/anh/them")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProductImage(long id, IFormFile image, string? altText, bool isPrimary = false, int sortOrder = 0)
        {
            var token = GetAdminToken();
            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var isSuccess = image != null && image.Length > 0
                && await _productService.AddProductImageAsync(id, image, altText, isPrimary, sortOrder, token);
            TempData[isSuccess ? "Success" : "Error"] = isSuccess
                ? "Da them anh san pham."
                : "Khong the them anh san pham.";

            return RedirectToAction(nameof(ProductDetails), new { id });
        }

        [HttpPost("san-pham/{id:long}/anh/{imageId:long}/primary")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPrimaryProductImage(long id, long imageId)
        {
            var token = GetAdminToken();
            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var isSuccess = await _productService.SetPrimaryProductImageAsync(id, imageId, token);
            TempData[isSuccess ? "Success" : "Error"] = isSuccess
                ? "Da chon anh chinh."
                : "Khong the chon anh chinh.";

            return RedirectToAction(nameof(ProductDetails), new { id });
        }

        [HttpPost("san-pham/{id:long}/anh/{imageId:long}/xoa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProductImage(long id, long imageId)
        {
            var token = GetAdminToken();
            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var isSuccess = await _productService.DeleteProductImageAsync(id, imageId, token);
            TempData[isSuccess ? "Success" : "Error"] = isSuccess
                ? "Da xoa anh san pham."
                : "Khong the xoa anh san pham.";

            return RedirectToAction(nameof(ProductDetails), new { id });
        }

        [HttpPost("san-pham/{id:long}/thong-so")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProductSpecifications(
            long id,
            string[] groupName,
            string[] specKey,
            string[] specValue,
            int[] sortOrder)
        {
            var token = GetAdminToken();
            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var specifications = new List<ProductSpecificationViewModel>();
            for (var i = 0; i < specKey.Length; i++)
            {
                var value = specValue.ElementAtOrDefault(i);
                if (string.IsNullOrWhiteSpace(specKey[i]) || string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                specifications.Add(new ProductSpecificationViewModel
                {
                    GroupName = groupName.ElementAtOrDefault(i) ?? "General",
                    SpecKey = specKey[i],
                    SpecValue = value,
                    SortOrder = sortOrder.ElementAtOrDefault(i)
                });
            }

            var isSuccess = await _productService.UpdateProductSpecificationsAsync(id, specifications, token);
            TempData[isSuccess ? "Success" : "Error"] = isSuccess
                ? "Da cap nhat thong so."
                : "Khong the cap nhat thong so.";

            return RedirectToAction(nameof(ProductDetails), new { id });
        }

        [HttpGet("don-hang")]
        public async Task<IActionResult> Orders()
        {
            var token = GetAdminToken();
            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var orders = await _orderService.GetOrdersAsync(token);

            return View(orders);
        }

        [HttpPost("don-hang/{id:long}/trang-thai")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(long id, string status)
        {
            var token = GetAdminToken();
            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var isSuccess = await _orderService.UpdateOrderStatusAsync(id, status, token);
            TempData[isSuccess ? "Success" : "Error"] = isSuccess
                ? "Da cap nhat trang thai don hang."
                : "Khong the cap nhat trang thai don hang.";

            return RedirectToAction(nameof(Orders));
        }

        [HttpGet("kho-hang")]
        public async Task<IActionResult> Inventory(string? status = null)
        {
            var token = GetAdminToken();
            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Status = status;
            var inventory = await _inventoryService.GetInventoryAsync(token, status);

            return View(inventory);
        }

        [HttpPost("kho-hang/{productId:long}/cap-nhat")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStock(long productId, UpdateStockDto dto)
        {
            var token = GetAdminToken();
            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var isSuccess = await _inventoryService.UpdateStockAsync(productId, dto, token);
            TempData[isSuccess ? "Success" : "Error"] = isSuccess
                ? "Da cap nhat ton kho."
                : "Khong the cap nhat ton kho.";

            return RedirectToAction(nameof(Inventory));
        }

        [HttpPost("kho-hang/{productId:long}/nhap")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportStock(long productId, ImportStockDto dto)
        {
            var token = GetAdminToken();
            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var isSuccess = await _inventoryService.ImportStockAsync(productId, dto, token);
            TempData[isSuccess ? "Success" : "Error"] = isSuccess
                ? "Da nhap them hang."
                : "Khong the nhap hang.";

            return RedirectToAction(nameof(Inventory));
        }

        [HttpGet("users")]
        public async Task<IActionResult> Users(string? search = null)
        {
            var token = GetAdminToken();
            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Search = search;
            var users = await _userService.GetUsersAsync(token, search);

            return View(users);
        }

        [HttpGet("users/{id:long}")]
        public async Task<IActionResult> UserDetails(long id)
        {
            var token = GetAdminToken();
            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _userService.GetUserAsync(id, token);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost("users/{id:long}/active")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserActive(long id, bool isActive)
        {
            var token = GetAdminToken();
            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var isSuccess = await _userService.UpdateActiveAsync(id, isActive, token);
            TempData[isSuccess ? "Success" : "Error"] = isSuccess
                ? "Da cap nhat tai khoan."
                : "Khong the cap nhat tai khoan.";

            return RedirectToAction(nameof(Users));
        }

        [HttpPost("users/{id:long}/role")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRole(long id, string role)
        {
            var token = GetAdminToken();
            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var isSuccess = await _userService.UpdateRoleAsync(id, role, token);
            TempData[isSuccess ? "Success" : "Error"] = isSuccess
                ? "Da cap nhat role."
                : "Khong the cap nhat role.";

            return RedirectToAction(nameof(Users));
        }

        [HttpPost("users/{id:long}/xoa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(long id)
        {
            var token = GetAdminToken();
            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var isSuccess = await _userService.DeleteUserAsync(id, token);
            TempData[isSuccess ? "Success" : "Error"] = isSuccess
                ? "Da xoa user."
                : "Khong the xoa user.";

            return RedirectToAction(nameof(Users));
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

        private string? GetAdminToken()
        {
            if (!IsAdmin())
            {
                return null;
            }

            return HttpContext.Session.GetString("JWT");
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }
    }
}
