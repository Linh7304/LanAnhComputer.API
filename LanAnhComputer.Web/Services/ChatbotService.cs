using LanAnhComputer.API.Dtos;
using System.Net.Http.Json;
using System.Text.Json;

namespace LanAnhComputer.Web.Services
{
    public class ChatbotService : IChatbotService
    {
        private readonly HttpClient _httpClient;

        public ChatbotService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> HandleChatAsync(ChatRequestDto request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/chatbot/ask", request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(content);
                if (jsonDoc.RootElement.TryGetProperty("response", out var respProp))
                {
                    return respProp.GetString() ?? "Xin lỗi, đã có lỗi kết nối tới máy chủ.";
                }
            }
            
            return "Xin lỗi, đã có lỗi xảy ra.";
        }
    }
}
