using Microsoft.AspNetCore.Mvc;

namespace LanAnhComputer.Web.Controllers
{
    public class OrdersController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> CheckPaymentStatus(long orderId)
        {
            var client = new HttpClient();

            var response = await client.GetAsync(
                $"https://localhost:7132/api/payment/status/{orderId}"
            );

            if (!response.IsSuccessStatusCode)
            {
                return Json(new
                {
                    success = false
                });
            }

            var result =
                await response.Content.ReadFromJsonAsync<dynamic>();

            return Json(new
            {
                success =
                    result?.paymentStatus?.ToString() == "PAID"
            });
        }
    }
}