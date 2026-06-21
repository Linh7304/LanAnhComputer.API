using LanAnhComputer.API.Dtos;
using LanAnhComputer.Data;
using LanAnhComputer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text;

namespace LanAnhComputer.API.Services
{
    public class ChatbotService : IChatbotService
    {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public ChatbotService(AppDbContext dbContext, IConfiguration configuration, HttpClient httpClient)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<string> HandleChatAsync(ChatRequestDto request)
        {
            // 1. Search for related products
            var productsContext = await SearchProductsForChatbotAsync(request.Message);
            
            // 2. Build context string
            var contextBuilder = new StringBuilder();
            if (productsContext.Any())
            {
                contextBuilder.AppendLine("\nDỮ LIỆU SẢN PHẨM (Từ cơ sở dữ liệu hệ thống):");
                foreach (var p in productsContext)
                {
                    contextBuilder.AppendLine($"- {p.ProductName} | Giá: {p.SalePrice:N0} VNĐ | Tồn kho: {p.StockQuantity}");
                }
            }
            else
            {
                contextBuilder.AppendLine("\nDỮ LIỆU SẢN PHẨM: Không tìm thấy sản phẩm nào khớp với yêu cầu.");
            }

            // 3. Prepare payload for Gemini
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("Gemini API Key is missing.");
            }

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";

            var systemInstructionText = "Bạn là trợ lý tư vấn của LanAnhComputer. " +
                "Chỉ được phép sử dụng DỮ LIỆU SẢN PHẨM được cung cấp phía dưới để trả lời. " +
                "Tuyệt đối KHÔNG ĐƯỢC tự tạo ra sản phẩm, cấu hình hoặc giá bán không tồn tại trong hệ thống. " +
                "Nếu không tìm thấy dữ liệu phù hợp, hãy thông báo rõ cho khách hàng là cửa hàng hiện không có sản phẩm đó và xin lỗi khách. " +
                "Hãy trả lời ngắn gọn, thân thiện và chuyên nghiệp.";

            var contentsList = new List<object>();

            // Add history
            foreach (var h in request.History)
            {
                contentsList.Add(new
                {
                    role = h.Role == "user" ? "user" : "model",
                    parts = new[] { new { text = h.Text } }
                });
            }

            // Add current message with context
            var finalUserMessage = request.Message + "\n\n" + contextBuilder.ToString();
            contentsList.Add(new
            {
                role = "user",
                parts = new[] { new { text = finalUserMessage } }
            });

            var payload = new
            {
                systemInstruction = new
                {
                    parts = new[] { new { text = systemInstructionText } }
                },
                contents = contentsList
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            string botResponse = "Xin lỗi, tôi không thể trả lời lúc này.";

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[Gemini Error Response]: {response.StatusCode} - {errorContent}");
                    throw new Exception($"Gemini API returned status {response.StatusCode}: {errorContent}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(responseString);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    botResponse = candidates[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? botResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Gemini Exception]: {ex}");
                botResponse = $"Xin lỗi, đã có lỗi xảy ra khi kết nối tới máy chủ AI. Chi tiết: {ex.Message}";
            }

            // 4. Save to ChatbotHistories
            var historyRecord = new ChatbotHistory
            {
                SessionId = string.IsNullOrWhiteSpace(request.SessionId) ? Guid.NewGuid().ToString() : request.SessionId,
                UserId = request.UserId,
                UserMessage = request.Message,
                BotResponse = botResponse,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.ChatbotHistories.Add(historyRecord);
            await _dbContext.SaveChangesAsync();

            return botResponse;
        }

        private async Task<List<Product>> SearchProductsForChatbotAsync(string keyword)
        {
            var words = keyword.Split(new[] { ' ', ',', '.', '?' }, StringSplitOptions.RemoveEmptyEntries)
                               .Where(w => w.Length > 2)
                               .ToList();

            var query = _dbContext.Products.Where(p => p.IsActive && p.StockQuantity > 0);
            
            // Bring to memory if small, or just evaluate. We'll do a simple Contains if possible.
            // A safe approach for EF core string search is bringing names to memory if the DB is not huge, 
            // but for safety we'll just try to use EF. Core 3.0+ doesn't translate client-side eval implicitly.
            
            var allActiveProducts = await query.Select(p => new { p.ProductId, p.ProductName, p.SalePrice, p.StockQuantity }).ToListAsync();

            if (!words.Any())
            {
                // return some popular ones or none
                return new List<Product>();
            }

            var matched = allActiveProducts
                .Where(p => words.Any(w => p.ProductName.Contains(w, StringComparison.OrdinalIgnoreCase)))
                .Take(5)
                .Select(p => new Product 
                { 
                    ProductName = p.ProductName, 
                    SalePrice = p.SalePrice, 
                    StockQuantity = p.StockQuantity 
                })
                .ToList();

            return matched;
        }
    }
}
