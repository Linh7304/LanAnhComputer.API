using LanAnhComputer.Dtos;
using LanAnhComputer.Web.ViewModels;
using System.Net.Http.Json;

namespace LanAnhComputer.Web.Services
{
    public interface IProductService
    {
        Task<PagedResultViewModel<ProductViewModel>?> GetProductsAsync(int pageNumber, int pageSize, string? sort);
        Task<ProductViewModel?> GetProductDetailsAsync(long id);
        Task<List<ProductViewModel>> GetHomeProductsAsync();
    }

    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;

        public ProductService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PagedResultViewModel<ProductViewModel>?> GetProductsAsync(int pageNumber, int pageSize, string? sort)
        {
            var url = $"api/Products?pageNumber={pageNumber}&pageSize={pageSize}&sort={sort}";

            return await _httpClient.GetFromJsonAsync<PagedResultViewModel<ProductViewModel>>(url);
        }

        public async Task<ProductViewModel?> GetProductDetailsAsync(long id)
        {
            return await _httpClient.GetFromJsonAsync<ProductViewModel>($"api/Products/{id}");
        }

        public async Task<List<ProductViewModel>> GetHomeProductsAsync()
        {
            var response = await _httpClient.GetAsync("api/Products");

            if (!response.IsSuccessStatusCode)
            {
                return new List<ProductViewModel>();
            }

            var pagedResult = await response.Content.ReadFromJsonAsync<PagedResultDto<ProductDto>>();

            return pagedResult?.Items?.Select(MapToViewModel).ToList() ?? new List<ProductViewModel>();
        }

        private static ProductViewModel MapToViewModel(ProductDto dto)
        {
            return new ProductViewModel
            {
                ProductId = dto.ProductId,
                ProductName = dto.ProductName,
                SalePrice = dto.SalePrice,
                OriginalPrice = dto.CostPrice,
                Rating = 0,
                Reviews = 0,
                Badge = dto.IsActive ? "Hot" : null,
                ImageUrl = dto.ImageUrl,
                Brand = dto.Brand ?? string.Empty
            };
        }
    }
}
