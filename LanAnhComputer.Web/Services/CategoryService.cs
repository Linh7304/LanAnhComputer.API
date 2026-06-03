using LanAnhComputer.Dtos;
using LanAnhComputer.Web.Extensions;
using System.Net.Http.Json;

namespace LanAnhComputer.Web.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetCategoriesAsync();
        Task<bool> CreateCategoryAsync(CategoryUpsertDto dto, string token);
        Task<bool> UpdateCategoryAsync(int id, CategoryUpsertDto dto, string token);
        Task<bool> DeleteCategoryAsync(int id, string token);
    }

    public class CategoryService : ICategoryService
    {
        private readonly HttpClient _httpClient;

        public CategoryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            var categories = await _httpClient.GetFromJsonAsync<List<CategoryDto>>("api/Categories");
            return categories ?? new List<CategoryDto>();
        }

        public async Task<bool> CreateCategoryAsync(CategoryUpsertDto dto, string token)
        {
            _httpClient.AddJwt(token);

            var response = await _httpClient.PostAsJsonAsync("api/Categories", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateCategoryAsync(int id, CategoryUpsertDto dto, string token)
        {
            _httpClient.AddJwt(token);

            var response = await _httpClient.PutAsJsonAsync($"api/Categories/{id}", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCategoryAsync(int id, string token)
        {
            _httpClient.AddJwt(token);

            var response = await _httpClient.DeleteAsync($"api/Categories/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
