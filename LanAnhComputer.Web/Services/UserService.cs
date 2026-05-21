using LanAnhComputer.Dtos;
using LanAnhComputer.Web.Extensions;
using System.Net.Http.Json;

namespace LanAnhComputer.Web.Services
{
    public interface IUserService
    {
        Task<List<UserDto>> GetUsersAsync(string token, string? search = null);
        Task<UserDto?> GetUserAsync(long id, string token);
        Task<bool> UpdateActiveAsync(long id, bool isActive, string token);
        Task<bool> UpdateRoleAsync(long id, string role, string token);
        Task<bool> DeleteUserAsync(long id, string token);
    }

    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;

        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<UserDto>> GetUsersAsync(string token, string? search = null)
        {
            _httpClient.AddJwt(token);

            var url = "api/Users";
            if (!string.IsNullOrWhiteSpace(search))
            {
                url += $"?search={Uri.EscapeDataString(search)}";
            }

            var users = await _httpClient.GetFromJsonAsync<List<UserDto>>(url);

            return users ?? new List<UserDto>();
        }

        public async Task<UserDto?> GetUserAsync(long id, string token)
        {
            _httpClient.AddJwt(token);

            return await _httpClient.GetFromJsonAsync<UserDto>($"api/Users/{id}");
        }

        public async Task<bool> UpdateActiveAsync(long id, bool isActive, string token)
        {
            _httpClient.AddJwt(token);

            var response = await _httpClient.PatchAsJsonAsync($"api/Users/{id}/active", new { isActive });

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateRoleAsync(long id, string role, string token)
        {
            _httpClient.AddJwt(token);

            var response = await _httpClient.PatchAsJsonAsync($"api/Users/{id}/role", new { role });

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUserAsync(long id, string token)
        {
            _httpClient.AddJwt(token);

            var response = await _httpClient.DeleteAsync($"api/Users/{id}");

            return response.IsSuccessStatusCode;
        }
    }
}
