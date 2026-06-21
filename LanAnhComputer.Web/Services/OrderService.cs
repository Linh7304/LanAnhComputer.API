using LanAnhComputer.Dtos;
using LanAnhComputer.Web.Extensions;

namespace LanAnhComputer.Web.Services
{
    public interface IOrderService
    {
        Task<List<OrderDto>> GetOrdersAsync(string token);
        Task<OrderDto?> GetOrderByIdAsync(long id, string token);
        Task<bool> UpdateOrderStatusAsync(long orderId, string status, string token);
    }

    public class OrderService : IOrderService
    {
        private readonly HttpClient _httpClient;

        public OrderService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<OrderDto>> GetOrdersAsync(string token)
        {
            _httpClient.AddJwt(token);

            var orders = await _httpClient.GetFromJsonAsync<List<OrderDto>>("api/Orders");

            return orders ?? new List<OrderDto>();
        }

        public async Task<OrderDto?> GetOrderByIdAsync(long id, string token)
        {
            _httpClient.AddJwt(token);

            try
            {
                var order = await _httpClient.GetFromJsonAsync<OrderDto>($"api/Orders/{id}");
                return order;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(long orderId, string status, string token)
        {
            _httpClient.AddJwt(token);

            var response = await _httpClient.PatchAsJsonAsync(
                $"api/Orders/{orderId}/status",
                new { orderStatus = status });

            return response.IsSuccessStatusCode;
        }
    }
}
