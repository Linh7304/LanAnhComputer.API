using LanAnhComputer.Dtos;
using LanAnhComputer.Web.Extensions;
using System.Net.Http.Json;

namespace LanAnhComputer.Web.Services
{
    public interface IOrderService
    {
        Task<List<OrderDto>> GetOrdersAsync(string token);
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
