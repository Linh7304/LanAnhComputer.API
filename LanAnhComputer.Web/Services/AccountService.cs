using LanAnhComputer.API.Dtos;
using LanAnhComputer.Web.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace LanAnhComputer.Web.Services
{
    public interface IAccountService
    {
        Task<AuthResponseViewModel?> LoginAsync(LoginViewModel model);

        Task<AuthResponseViewModel?> RegisterAsync(RegisterViewModel model);

        Task<UserProfileDtos?> GetProfileAsync(string token);

        Task<bool> UpdateProfileAsync(
            ProfileViewModel model,
            string token);

        Task<bool> ChangePasswordAsync(
            ProfileViewModel model,
            string token);
    }

    public class AccountService : IAccountService
    {
        private readonly HttpClient _httpClient;

        public AccountService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AuthResponseViewModel?> LoginAsync(
            LoginViewModel model)
        {
            var response =
                await _httpClient.PostAsJsonAsync(
                    "api/Auth/login",
                    model);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content
                .ReadFromJsonAsync<AuthResponseViewModel>();
        }

        public async Task<AuthResponseViewModel?> RegisterAsync(
            RegisterViewModel model)
        {
            var response =
                await _httpClient.PostAsJsonAsync(
                    "api/Auth/register",
                    model);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content
                .ReadFromJsonAsync<AuthResponseViewModel>();
        }

        public async Task<UserProfileDtos?> GetProfileAsync(
            string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            return await _httpClient.GetFromJsonAsync<UserProfileDtos>(
                "api/account/profile");
        }

        public async Task<bool> UpdateProfileAsync(
            ProfileViewModel model,
            string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            var response =
                await _httpClient.PutAsJsonAsync(
                    "api/account/profile",
                    new
                    {
                        model.FullName,
                        model.Phone,
                        model.Gender,
                        model.DateOfBirth
                    });

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ChangePasswordAsync(
            ProfileViewModel model,
            string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            var response =
                await _httpClient.PutAsJsonAsync(
                    "api/account/change-password",
                    new
                    {
                        model.CurrentPassword,
                        model.NewPassword,
                        model.ConfirmPassword
                    });

            return response.IsSuccessStatusCode;
        }
    }
}