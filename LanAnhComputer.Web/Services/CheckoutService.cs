using LanAnhComputer.API.Dtos;
using LanAnhComputer.Web.Extensions;
using LanAnhComputer.Web.Models;
using LanAnhComputer.Web.ViewModels;
using System.Net.Http.Json;

namespace LanAnhComputer.Web.Services
{
    public interface ICheckoutService
    {
        Task<OrderResultViewModel?> PlaceOrderAsync(
            CheckoutViewModel model,
            string token
        );
        Task<PayOSResponseViewModel?> CreatePaymentAsync(
            long orderId,
            string token
        );
        Task<bool> CheckPaymentStatusAsync(
            long orderId,
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

        public async Task<OrderResultViewModel?> PlaceOrderAsync(CheckoutViewModel model,string token)
        {
            _httpClient.AddJwt(token);
            Console.WriteLine("CALL API CREATE ORDER"); //debug
            
            var dto = new CheckoutDtos
            {
                ShippingFullName = model.FullName,
                ShippingPhone = model.Phone,
                ShippingAddressLine = model.Address,
                ShippingCity = model.Province,
                ShippingWard = model.Ward,
                PaymentMethod = model.PaymentMethod,
                Note = model.Note,
                DiscountAmount = 0,
                ShippingFee = 0
            };

            var response = await _httpClient.PostAsJsonAsync("api/Orders/checkout", dto);
            // DEBUG RESPONSE BODY
            Console.WriteLine($"STATUS: {response.StatusCode}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<OrderResultViewModel>();

            return result;
        }

        public async Task<PayOSResponseViewModel?> CreatePaymentAsync(long orderId, string token)
        {
            _httpClient.AddJwt(token);

            var response = await _httpClient.PostAsync($"api/payment/create/{orderId}",null);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content
                .ReadFromJsonAsync<PayOSResponseViewModel>();

            return result;
        }

        public async Task<bool> CheckPaymentStatusAsync(long orderId,string token)
        {
            _httpClient.AddJwt(token);

            var response = await _httpClient.GetAsync($"api/payment/status/{orderId}");

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<PaymentStatusResponse>();


            return string.Equals(result?.PaymentStatus, "PAID", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class PaymentStatusResponse
    {
        public string PaymentStatus { get; set; } = "";
    }
}