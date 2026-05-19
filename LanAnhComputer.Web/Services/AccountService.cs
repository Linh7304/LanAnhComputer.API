using LanAnhComputer.Web.Models;
using System.Net.Http.Json;

namespace LanAnhComputer.Web.Services
{
    public interface IAccountService
    {
        Task<AuthResponseViewModel?> LoginAsync(LoginViewModel model);
        Task<AuthResponseViewModel?> RegisterAsync(RegisterViewModel model);
    }

    public class AccountService : IAccountService
    {
        private readonly HttpClient _httpClient;

        public AccountService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AuthResponseViewModel?> LoginAsync(LoginViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/login", model);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<AuthResponseViewModel>();
        }

        public async Task<AuthResponseViewModel?> RegisterAsync(RegisterViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/register", model);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<AuthResponseViewModel>();
        }
    }
}
