using LanAnhComputer.Dtos;
using LanAnhComputer.Web.Extensions;
using System.Net.Http.Json;

namespace LanAnhComputer.Web.Services
{
    public interface IAdminInventoryService
    {
        Task<List<InventoryItemDto>> GetInventoryAsync(string token, string? status = null);
        Task<InventorySummaryDto?> GetSummaryAsync(string token);
        Task<bool> UpdateStockAsync(long productId, UpdateStockDto dto, string token);
        Task<bool> ImportStockAsync(long productId, ImportStockDto dto, string token);
    }

    public class AdminInventoryService : IAdminInventoryService
    {
        private readonly HttpClient _httpClient;

        public AdminInventoryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<InventoryItemDto>> GetInventoryAsync(string token, string? status = null)
        {
            _httpClient.AddJwt(token);

            var url = "api/Inventory";
            if (!string.IsNullOrWhiteSpace(status))
            {
                url += $"?status={Uri.EscapeDataString(status)}";
            }

            var items = await _httpClient.GetFromJsonAsync<List<InventoryItemDto>>(url);

            return items ?? new List<InventoryItemDto>();
        }

        public async Task<InventorySummaryDto?> GetSummaryAsync(string token)
        {
            _httpClient.AddJwt(token);

            return await _httpClient.GetFromJsonAsync<InventorySummaryDto>("api/Inventory/summary");
        }

        public async Task<bool> UpdateStockAsync(long productId, UpdateStockDto dto, string token)
        {
            _httpClient.AddJwt(token);

            var response = await _httpClient.PutAsJsonAsync($"api/Inventory/{productId}/stock", dto);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ImportStockAsync(long productId, ImportStockDto dto, string token)
        {
            _httpClient.AddJwt(token);

            var response = await _httpClient.PostAsJsonAsync($"api/Inventory/{productId}/import", dto);

            return response.IsSuccessStatusCode;
        }
    }
}
