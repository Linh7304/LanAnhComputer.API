using LanAnhComputer.Dtos;
using LanAnhComputer.Web.Extensions;
using LanAnhComputer.Web.Models;
using LanAnhComputer.Web.ViewModels;
using System.Globalization;
using System.Net.Http.Json;

namespace LanAnhComputer.Web.Services
{
    public interface IProductService
    {
        Task<PagedResultViewModel<ProductViewModel>?> GetProductsAsync(int pageNumber, int pageSize, string? sort);
        Task<PagedResultViewModel<ProductViewModel>?> GetProductsAsync(int pageNumber, int pageSize, string? sort, string? search);
        Task<ProductViewModel?> GetProductDetailsAsync(long id);
        Task<List<ProductViewModel>> GetHomeProductsAsync();
        Task<bool> SubmitReviewAsync(long productId, int rating, string? comment, string token);
        Task<bool> CreateProductAsync(AdminProductFormViewModel model, string token);
        Task<bool> UpdateProductAsync(AdminProductFormViewModel model, string token);
        Task<bool> DeleteProductAsync(long id, string token);
        Task<bool> AddProductImageAsync(long productId, IFormFile image, string? altText, bool isPrimary, int sortOrder, string token);
        Task<bool> SetPrimaryProductImageAsync(long productId, long imageId, string token);
        Task<bool> DeleteProductImageAsync(long productId, long imageId, string token);
        Task<bool> UpdateProductSpecificationsAsync(long productId, List<ProductSpecificationViewModel> specifications, string token);
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
            return await GetProductsAsync(pageNumber, pageSize, sort, null);
        }

        public async Task<PagedResultViewModel<ProductViewModel>?> GetProductsAsync(int pageNumber, int pageSize, string? sort, string? search)
        {
            var url = $"api/Products?pageNumber={pageNumber}&pageSize={pageSize}&sort={sort}";

            if (!string.IsNullOrWhiteSpace(search))
            {
                url += $"&search={Uri.EscapeDataString(search)}";
            }

            return await _httpClient.GetFromJsonAsync<PagedResultViewModel<ProductViewModel>>(url);
        }

        public async Task<ProductViewModel?> GetProductDetailsAsync(long id)
        {
            return await _httpClient.GetFromJsonAsync<ProductViewModel>($"api/Products/{id}");
        }

        public async Task<bool> SubmitReviewAsync(long productId, int rating, string? comment, string token)
        {
            _httpClient.AddJwt(token);

            var response = await _httpClient.PostAsJsonAsync(
                $"api/products/{productId}/reviews",
                new { rating, comment });

            return response.IsSuccessStatusCode;
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

        public async Task<bool> CreateProductAsync(AdminProductFormViewModel model, string token)
        {
            _httpClient.AddJwt(token);

            using var content = BuildProductFormData(model);
            var response = await _httpClient.PostAsync("api/Products", content);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateProductAsync(AdminProductFormViewModel model, string token)
        {
            _httpClient.AddJwt(token);

            using var content = BuildProductFormData(model);
            var response = await _httpClient.PutAsync($"api/Products/{model.ProductId}", content);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteProductAsync(long id, string token)
        {
            _httpClient.AddJwt(token);

            var response = await _httpClient.DeleteAsync($"api/Products/{id}");

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AddProductImageAsync(long productId, IFormFile image, string? altText, bool isPrimary, int sortOrder, string token)
        {
            _httpClient.AddJwt(token);

            using var content = new MultipartFormDataContent
            {
                { ToStringContent(altText), "AltText" },
                { ToStringContent(isPrimary), "IsPrimary" },
                { ToStringContent(sortOrder), "SortOrder" }
            };

            var fileContent = new StreamContent(image.OpenReadStream());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(image.ContentType);
            content.Add(fileContent, "Image", image.FileName);

            var response = await _httpClient.PostAsync($"api/Products/{productId}/images", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SetPrimaryProductImageAsync(long productId, long imageId, string token)
        {
            _httpClient.AddJwt(token);

            var response = await _httpClient.PutAsync($"api/Products/{productId}/images/{imageId}/primary", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteProductImageAsync(long productId, long imageId, string token)
        {
            _httpClient.AddJwt(token);

            var response = await _httpClient.DeleteAsync($"api/Products/{productId}/images/{imageId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateProductSpecificationsAsync(long productId, List<ProductSpecificationViewModel> specifications, string token)
        {
            _httpClient.AddJwt(token);

            var payload = specifications.Select(x => new
            {
                groupName = x.GroupName,
                specKey = x.SpecKey,
                specValue = x.SpecValue,
                sortOrder = x.SortOrder
            });

            var response = await _httpClient.PutAsJsonAsync($"api/Products/{productId}/specifications", payload);
            return response.IsSuccessStatusCode;
        }

        private static ProductViewModel MapToViewModel(ProductDto dto)
        {
            return new ProductViewModel
            {
                ProductId = dto.ProductId,
                ProductName = dto.ProductName,
                SalePrice = dto.SalePrice,
                OriginalPrice = dto.CostPrice,
                Rating = (double)dto.AverageRating,
                Reviews = dto.TotalReviews,
                Badge = dto.IsActive ? "Hot" : null,
                ImageUrl = dto.ThumbnailUrl ?? dto.ImageUrl,
                ThumbnailUrl = dto.ThumbnailUrl,
                Brand = dto.Brand ?? string.Empty,
                ShortDescription = dto.ShortDescription,
                Description = dto.Description,
                StockQuantity = dto.StockQuantity,
                AverageRating = dto.AverageRating,
                TotalReviews = dto.TotalReviews,
                SoldQuantity = dto.SoldQuantity
            };
        }

        private static MultipartFormDataContent BuildProductFormData(AdminProductFormViewModel model)
        {
            var content = new MultipartFormDataContent
            {
                { ToStringContent(model.CategoryId), nameof(model.CategoryId) },
                { ToStringContent(model.ProductCode), nameof(model.ProductCode) },
                { ToStringContent(model.ProductName), nameof(model.ProductName) },
                { ToStringContent(model.ProductType), nameof(model.ProductType) },
                { ToStringContent(model.Brand), nameof(model.Brand) },
                { ToStringContent(model.Model), nameof(model.Model) },
                { ToStringContent(model.ShortDescription), nameof(model.ShortDescription) },
                { ToStringContent(model.Description), nameof(model.Description) },
                { ToStringContent(model.ThumbnailUrl), nameof(model.ThumbnailUrl) },
                { ToStringContent(model.Specifications), nameof(model.Specifications) },
                { ToStringContent(model.WarrantyMonths), nameof(model.WarrantyMonths) },
                { ToStringContent(model.CostPrice), nameof(model.CostPrice) },
                { ToStringContent(model.SalePrice), nameof(model.SalePrice) },
                { ToStringContent(model.StockQuantity), nameof(model.StockQuantity) },
                { ToStringContent(model.ReorderLevel), nameof(model.ReorderLevel) },
                { ToStringContent(model.ImageUrl), nameof(model.ImageUrl) },
                { ToStringContent(model.IsActive), nameof(model.IsActive) }
            };

            if (model.Image != null && model.Image.Length > 0)
            {
                var fileContent = new StreamContent(model.Image.OpenReadStream());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.Image.ContentType);
                content.Add(fileContent, nameof(model.Image), model.Image.FileName);
            }

            return content;
        }

        private static StringContent ToStringContent(object? value)
        {
            var stringValue = value switch
            {
                null => string.Empty,
                decimal decimalValue => decimalValue.ToString(CultureInfo.InvariantCulture),
                bool boolValue => boolValue.ToString(),
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString() ?? string.Empty
            };

            return new StringContent(stringValue);
        }
    }
}
