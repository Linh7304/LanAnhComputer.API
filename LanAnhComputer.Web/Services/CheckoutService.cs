using LanAnhComputer.API.Dtos;
using LanAnhComputer.Web.Extensions;
using LanAnhComputer.Web.Models;
using LanAnhComputer.Web.ViewModels;

namespace LanAnhComputer.Web.Services
{
    public interface ICheckoutService
    {
        Task<OrderResultViewModel?> PlaceOrderAsync(
            CheckoutViewModel model,
            string token
        );
    }

    public class CheckoutService : ICheckoutService
    {
        private readonly HttpClient _httpClient;

        public CheckoutService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<OrderResultViewModel?> PlaceOrderAsync(
            CheckoutViewModel model,
            string token)
        {
            _httpClient.AddJwt(token);

            var dto = new CheckoutDtos
            {
                ShippingFullName = model.FullName,
                ShippingPhone = model.Phone,
                ShippingAddressLine = model.Address,
                ShippingCity = model.Province,
                ShippingDistrict = model.District,
                ShippingWard = model.Ward,
                PaymentMethod = model.PaymentMethod,
                Note = model.Note,
                DiscountAmount = 0,
                ShippingFee = 0
            };

            var response = await _httpClient.PostAsJsonAsync(
                "api/Orders/checkout",
                dto
            );

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content
                .ReadFromJsonAsync<OrderResultViewModel>();

            return result;
        }
    }
}