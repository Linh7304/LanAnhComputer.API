using LanAnhComputer.API.Dtos;
using LanAnhComputer.Web.Extensions;
using LanAnhComputer.Web.Models;
using LanAnhComputer.Web.ViewModels;
using System.Net.Http.Json;

namespace LanAnhComputer.Web.Services
{
    public interface ICartService
    {
        Task<List<CartItemViewModel>> GetCartItemsAsync(string token);
        Task<bool> AddToCartAsync(AddToCartRequestViewModel request, string token);
    }

    public class CartService : ICartService
    {
        private readonly HttpClient _httpClient;

        public CartService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<CartItemViewModel>> GetCartItemsAsync(string token)
        {
            _httpClient.AddJwt(token);

            var dtoList = await _httpClient.GetFromJsonAsync<List<CartItemDto>>("api/cart");

            return dtoList?.Select(MapToViewModel).ToList() ?? new List<CartItemViewModel>();
        }

        public async Task<bool> AddToCartAsync(AddToCartRequestViewModel request, string token)
        {
            _httpClient.AddJwt(token);

            var response = await _httpClient.PostAsJsonAsync("api/cart/add", request);

            return response.IsSuccessStatusCode;
        }

        private static CartItemViewModel MapToViewModel(CartItemDto dto)
        {
            return new CartItemViewModel
            {
                ProductId = dto.ProductId,
                ProductName = dto.ProductName,
                Price = dto.Price,
                Quantity = dto.Quantity,
                ImageUrl = dto.ImageUrl,
                Brand = dto.Brand,
                ProductType = dto.ProductType
            };
        }
    }
}
