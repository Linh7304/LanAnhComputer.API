using LanAnhComputer.API.Dtos;
using LanAnhComputer.Data;
using LanAnhComputer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LanAnhComputer.API.Services
{
    public class ChatbotService : IChatbotService
    {
        private const int MaxProductsInContext = 5;

        private static readonly HashSet<string> ProductRelatedIntents = new(StringComparer.OrdinalIgnoreCase)
        {
            "tim_san_pham",
            "tu_van_san_pham",
            "so_sanh_san_pham",
            "kiem_tra_gia",
            "kiem_tra_ton_kho",
            "goi_y_cau_hinh"
        };

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

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
            string botResponse;

            try
            {
                var intent = await DetectIntentAsync(request);
                var products = IsProductRelatedIntent(intent.Intent)
                    ? await SearchProductsAsync(intent)
                    : new List<ChatProductContextItem>();

                var productContext = BuildProductContext(products, intent.Intent);
                botResponse = await GenerateFinalResponseAsync(request, intent, productContext);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Chatbot Exception]: {ex}");
                botResponse = $"Xin lỗi, đã có lỗi xảy ra khi kết nối tới máy chủ AI. Chi tiết: {ex.Message}";
            }

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

        private async Task<ChatIntentDto> DetectIntentAsync(ChatRequestDto request)
        {
            var historySummary = BuildRecentHistorySummary(request.History);
            var historyPart = string.IsNullOrWhiteSpace(historySummary)
                ? string.Empty
                : $"Ngữ cảnh hội thoại gần đây:\n{historySummary}\n";

            var prompt =
                "Bạn là bộ phân loại ý định (intent) cho chatbot thương mại điện tử LanAnhComputer.\n" +
                "Phân tích tin nhắn khách hàng và trả về JSON duy nhất theo schema sau (không markdown, không giải thích):\n\n" +
                "{\n" +
                "  \"intent\": \"chao_hoi\" | \"tim_san_pham\" | \"tu_van_san_pham\" | \"so_sanh_san_pham\" | \"kiem_tra_gia\" | \"kiem_tra_ton_kho\" | \"goi_y_cau_hinh\" | \"khac\",\n" +
                "  \"search_keyword\": \"\",\n" +
                "  \"brand\": \"\",\n" +
                "  \"product_type\": \"\",\n" +
                "  \"budget_min\": null,\n" +
                "  \"budget_max\": null\n" +
                "}\n\n" +
                "Quy tắc:\n" +
                "- intent \"chao_hoi\": chào hỏi, cảm ơn, trò chuyện xã giao không liên quan sản phẩm.\n" +
                "- intent \"tim_san_pham\": tìm/ hỏi có sản phẩm nào.\n" +
                "- intent \"tu_van_san_pham\": hỏi tư vấn chọn mua, phù hợp nhu cầu.\n" +
                "- intent \"so_sanh_san_pham\": so sánh 2+ sản phẩm.\n" +
                "- intent \"kiem_tra_gia\": hỏi giá cụ thể.\n" +
                "- intent \"kiem_tra_ton_kho\": hỏi còn hàng/ tồn kho.\n" +
                "- intent \"goi_y_cau_hinh\": hỏi cấu hình theo ngân sách/ mục đích sử dụng.\n" +
                "- intent \"khac\": câu hỏi chung không thuộc các loại trên.\n" +
                "- search_keyword: từ khóa sản phẩm (tên, model, dòng máy). Để rỗng nếu không có.\n" +
                "- brand: thương hiệu nếu khách nhắc (VD: ASUS, Dell, HP). Để rỗng nếu không có.\n" +
                "- product_type: loại sản phẩm (VD: laptop, màn hình, chuột). Để rỗng nếu không có.\n" +
                "- budget_min / budget_max: số tiền VNĐ (không có đơn vị). null nếu không đề cập.\n\n" +
                historyPart +
                $"Tin nhắn hiện tại: {request.Message}";

            var responseText = await CallGeminiAsync(
                systemInstruction: "Chỉ trả về JSON hợp lệ theo schema đã cho. Không thêm text ngoài JSON.",
                contents: new[] { new GeminiContent("user", prompt) },
                responseMimeType: "application/json");

            var intent = ParseIntentResponse(responseText);
            Console.WriteLine($"[Chatbot Intent]: {intent.Intent} | keyword={intent.SearchKeyword} | brand={intent.Brand} | type={intent.ProductType} | budget={intent.BudgetMin}-{intent.BudgetMax}");
            return intent;
        }

        private async Task<List<ChatProductContextItem>> SearchProductsAsync(ChatIntentDto intent)
        {
            var results = await ExecuteProductQueryAsync(intent, applyPriceFilter: true);

            if (results.Count == 0 && (intent.BudgetMin.HasValue || intent.BudgetMax.HasValue))
            {
                results = await SearchNearestByPriceAsync(intent);
            }

            if (results.Count == 0 && HasSearchCriteria(intent))
            {
                results = await ExecuteProductQueryAsync(intent, applyPriceFilter: false);
            }

            return results;
        }

        private async Task<List<ChatProductContextItem>> ExecuteProductQueryAsync(ChatIntentDto intent, bool applyPriceFilter)
        {
            var query = _dbContext.Products
                .AsNoTracking()
                .Where(p => p.IsActive);

            if (!string.Equals(intent.Intent, "kiem_tra_ton_kho", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.StockQuantity > 0);
            }

            query = ApplyIntentFilters(query, intent, applyPriceFilter);

            return await ProjectProductContextItems(query)
                .Take(MaxProductsInContext)
                .ToListAsync();
        }

        private async Task<List<ChatProductContextItem>> SearchNearestByPriceAsync(ChatIntentDto intent)
        {
            var targetPrice = ResolveTargetPrice(intent);
            if (targetPrice <= 0)
            {
                return new List<ChatProductContextItem>();
            }

            var query = _dbContext.Products
                .AsNoTracking()
                .Where(p => p.IsActive);

            if (!string.Equals(intent.Intent, "kiem_tra_ton_kho", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.StockQuantity > 0);
            }

            query = ApplyIntentFilters(query, intent, applyPriceFilter: false);

            return await ProjectProductContextItems(query)
                .OrderBy(p => Math.Abs(p.SalePrice - targetPrice))
                .Take(MaxProductsInContext)
                .ToListAsync();
        }

        private static IQueryable<Product> ApplyIntentFilters(IQueryable<Product> query, ChatIntentDto intent, bool applyPriceFilter)
        {
            if (!string.IsNullOrWhiteSpace(intent.Brand))
            {
                var brand = intent.Brand.Trim();
                query = query.Where(p => p.Brand != null && EF.Functions.Like(p.Brand, $"%{brand}%"));
            }

            if (!string.IsNullOrWhiteSpace(intent.ProductType))
            {
                var productType = intent.ProductType.Trim();
                query = query.Where(p => EF.Functions.Like(p.ProductType, $"%{productType}%"));
            }

            if (!string.IsNullOrWhiteSpace(intent.SearchKeyword))
            {
                var keyword = intent.SearchKeyword.Trim();
                query = query.Where(p =>
                    EF.Functions.Like(p.ProductName, $"%{keyword}%") ||
                    (p.Model != null && EF.Functions.Like(p.Model, $"%{keyword}%")) ||
                    (p.Brand != null && EF.Functions.Like(p.Brand, $"%{keyword}%")) ||
                    EF.Functions.Like(p.ProductType, $"%{keyword}%"));
            }

            if (applyPriceFilter)
            {
                if (intent.BudgetMin.HasValue)
                {
                    query = query.Where(p => p.SalePrice >= intent.BudgetMin.Value);
                }

                if (intent.BudgetMax.HasValue)
                {
                    query = query.Where(p => p.SalePrice <= intent.BudgetMax.Value);
                }
            }

            return query;
        }

        private static IQueryable<ChatProductContextItem> ProjectProductContextItems(IQueryable<Product> query)
        {
            return query
                .OrderByDescending(p => p.SoldQuantity)
                .ThenByDescending(p => p.ViewCount)
                .Select(p => new ChatProductContextItem
                {
                    ProductName = p.ProductName,
                    Brand = p.Brand,
                    Model = p.Model,
                    ProductType = p.ProductType,
                    SalePrice = p.SalePrice,
                    StockQuantity = p.StockQuantity,
                    WarrantyMonths = p.WarrantyMonths,
                    Specifications = p.Specifications,
                    DynamicSpecifications = p.ProductSpecifications
                        .OrderBy(s => s.SortOrder)
                        .Select(s => s.SpecKey + ": " + s.SpecValue)
                        .ToList()
                });
        }

        private static string BuildProductContext(IReadOnlyList<ChatProductContextItem> products, string intent)
        {
            if (!IsProductRelatedIntent(intent))
            {
                return string.Empty;
            }

            if (products.Count == 0)
            {
                return "=== DỮ LIỆU SẢN PHẨM ===\nKhông tìm thấy sản phẩm khớp trực tiếp với yêu cầu. Hãy đề xuất các sản phẩm gần nhất nếu có trong phần dữ liệu bổ sung hoặc thông báo rõ là hệ thống chưa có sản phẩm phù hợp.";
            }

            var sb = new StringBuilder();
            sb.AppendLine("=== DỮ LIỆU SẢN PHẨM (từ cơ sở dữ liệu hệ thống) ===");

            for (var i = 0; i < products.Count; i++)
            {
                var product = products[i];
                sb.AppendLine($"[{i + 1}] {product.ProductName}");
                sb.AppendLine($"  - Thương hiệu: {product.Brand ?? "Không rõ"}");
                sb.AppendLine($"  - Model: {product.Model ?? "Không rõ"}");
                sb.AppendLine($"  - Loại: {product.ProductType ?? "Không rõ"}");
                sb.AppendLine($"  - Giá bán: {product.SalePrice:N0} VNĐ");
                sb.AppendLine($"  - Tồn kho: {product.StockQuantity}");
                sb.AppendLine($"  - Bảo hành: {product.WarrantyMonths} tháng");
                sb.AppendLine($"  - Thông số: {FormatSpecifications(product)}");
            }

            return sb.ToString().TrimEnd();
        }

        private async Task<string> GenerateFinalResponseAsync(ChatRequestDto request, ChatIntentDto intent, string productContext)
        {
            var systemInstruction = BuildSystemInstruction(intent.Intent);
            var contents = new List<GeminiContent>();

            foreach (var historyItem in request.History)
            {
                var role = historyItem.Role == "user" ? "user" : "model";
                contents.Add(new GeminiContent(role, historyItem.Text));
            }

            if (!string.IsNullOrWhiteSpace(productContext))
            {
                contents.Add(new GeminiContent("user", productContext));
                contents.Add(new GeminiContent("model", "Đã nhận dữ liệu sản phẩm từ hệ thống."));
            }

            contents.Add(new GeminiContent("user", request.Message));

            var response = await CallGeminiAsync(systemInstruction, contents);
            return string.IsNullOrWhiteSpace(response)
                ? "Xin lỗi, tôi không thể trả lời lúc này."
                : response;
        }

        private static string BuildSystemInstruction(string intent)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Bạn là trợ lý tư vấn của LanAnhComputer — cửa hàng máy tính và linh kiện.");
            sb.AppendLine("Trả lời bằng tiếng Việt, ngắn gọn, thân thiện và chuyên nghiệp.");
            sb.AppendLine();

            if (IsProductRelatedIntent(intent))
            {
                sb.AppendLine("Quy tắc khi khách hỏi về sản phẩm:");
                sb.AppendLine("- CHỈ được sử dụng dữ liệu trong phần DỮ LIỆU SẢN PHẨM đã cung cấp.");
                sb.AppendLine("- TUYỆT ĐỐI KHÔNG tự bịa tên sản phẩm, giá bán, tồn kho hoặc thông số kỹ thuật.");
                sb.AppendLine("- Nếu không có sản phẩm khớp chính xác, hãy đề xuất sản phẩm gần nhất trong dữ liệu (về giá, loại, thương hiệu) và nói rõ đó là gợi ý thay thế.");
                sb.AppendLine("- Khi so sánh sản phẩm, chỉ so sánh các sản phẩm có trong dữ liệu.");
                sb.AppendLine("- Ghi rõ giá và tình trạng tồn kho khi khách hỏi về giá hoặc còn hàng.");
            }
            else
            {
                sb.AppendLine("Quy tắc khi câu hỏi ngoài phạm vi sản phẩm:");
                sb.AppendLine("- Bạn có thể trả lời như trợ lý AI thông thường (chào hỏi, hướng dẫn mua hàng chung, chính sách cơ bản).");
                sb.AppendLine("- Nếu khách hỏi chi tiết sản phẩm cụ thể mà chưa có dữ liệu, hãy mời khách mô tả nhu cầu để bạn tra cứu.");
                sb.AppendLine("- Vẫn KHÔNG được bịa tên sản phẩm, giá bán hoặc tồn kho của LanAnhComputer.");
            }

            return sb.ToString().TrimEnd();
        }

        private async Task<string> CallGeminiAsync(
            string systemInstruction,
            IReadOnlyList<GeminiContent> contents,
            string? responseMimeType = null)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("Gemini API Key is missing.");
            }

            var model = _configuration["Gemini:Model"] ?? "gemini-2.5-flash";
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            var payload = new Dictionary<string, object>
            {
                ["systemInstruction"] = new
                {
                    parts = new[] { new { text = systemInstruction } }
                },
                ["contents"] = contents.Select(c => new
                {
                    role = c.Role,
                    parts = new[] { new { text = c.Text } }
                }).ToArray()
            };

            if (!string.IsNullOrWhiteSpace(responseMimeType))
            {
                payload["generationConfig"] = new
                {
                    responseMimeType = responseMimeType
                };
            }

            var jsonPayload = JsonSerializer.Serialize(payload);
            using var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, httpContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[Gemini Error Response]: {response.StatusCode} - {errorContent}");
                throw new InvalidOperationException($"Gemini API returned status {response.StatusCode}: {errorContent}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            return ExtractGeminiText(responseString);
        }

        private static string ExtractGeminiText(string responseString)
        {
            using var jsonDoc = JsonDocument.Parse(responseString);
            var root = jsonDoc.RootElement;

            if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                return candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? string.Empty;
            }

            return string.Empty;
        }

        private static ChatIntentDto ParseIntentResponse(string responseText)
        {
            if (string.IsNullOrWhiteSpace(responseText))
            {
                return new ChatIntentDto { Intent = "khac" };
            }

            try
            {
                var cleaned = responseText.Trim();
                if (cleaned.StartsWith("```"))
                {
                    cleaned = cleaned
                        .Replace("```json", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace("```", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Trim();
                }

                var intent = JsonSerializer.Deserialize<ChatIntentDto>(cleaned, JsonOptions);
                return intent ?? new ChatIntentDto { Intent = "khac" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Chatbot Intent Parse Error]: {ex.Message} | Raw: {responseText}");
                return new ChatIntentDto { Intent = "khac", SearchKeyword = responseText };
            }
        }

        private static string BuildRecentHistorySummary(IReadOnlyList<ChatMessageDto> history)
        {
            if (history.Count == 0)
            {
                return string.Empty;
            }

            var recent = history.TakeLast(4);
            return string.Join("\n", recent.Select(h => $"{h.Role}: {h.Text}"));
        }

        private static bool IsProductRelatedIntent(string intent)
            => ProductRelatedIntents.Contains(intent);

        private static bool HasSearchCriteria(ChatIntentDto intent)
            => !string.IsNullOrWhiteSpace(intent.SearchKeyword)
               || !string.IsNullOrWhiteSpace(intent.Brand)
               || !string.IsNullOrWhiteSpace(intent.ProductType)
               || intent.BudgetMin.HasValue
               || intent.BudgetMax.HasValue;

        private static decimal ResolveTargetPrice(ChatIntentDto intent)
        {
            if (intent.BudgetMin.HasValue && intent.BudgetMax.HasValue)
            {
                return (intent.BudgetMin.Value + intent.BudgetMax.Value) / 2;
            }

            return intent.BudgetMax ?? intent.BudgetMin ?? 0;
        }

        private static string FormatSpecifications(ChatProductContextItem product)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(product.Specifications))
            {
                parts.Add(product.Specifications.Trim());
            }

            if (product.DynamicSpecifications.Count > 0)
            {
                parts.Add(string.Join("; ", product.DynamicSpecifications));
            }

            return parts.Count > 0 ? string.Join(" | ", parts) : "Chưa có thông số chi tiết";
        }

        private sealed record GeminiContent(string Role, string Text);

        private sealed class ChatProductContextItem
        {
            public string ProductName { get; set; } = string.Empty;
            public string? Brand { get; set; }
            public string? Model { get; set; }
            public string? ProductType { get; set; }
            public decimal SalePrice { get; set; }
            public int StockQuantity { get; set; }
            public int WarrantyMonths { get; set; }
            public string? Specifications { get; set; }
            public List<string> DynamicSpecifications { get; set; } = new();
        }
    }
}
