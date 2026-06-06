using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GAMINGSTORE.Data;
using GAMINGSTORE.Models;
using Microsoft.EntityFrameworkCore;

namespace GAMINGSTORE.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GeminiService> _logger;

        public GeminiService(
            HttpClient httpClient,
            IConfiguration configuration,
            ApplicationDbContext context,
            ILogger<GeminiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }

        public async Task<ChatResponseDto> GetConsultationAsync(string userMessage, string? userId = null)
        {
            try
            {
                ApplicationUser? user = null;
                if (!string.IsNullOrEmpty(userId))
                {
                    user = await _context.Users
                        .Include(u => u.Orders)
                        .Include(u => u.LoyaltyPoints)
                        .FirstOrDefaultAsync(u => u.Id == userId);
                }

                var categories = await _context.Categories
                    .Where(c => c.IsMenuVisible)
                    .Select(c => c.Name)
                    .ToListAsync();

                var apiKey = _configuration["Gemini:ApiKey"];
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    var geminiResponse = await CallGeminiApiAsync(userMessage, user, categories, apiKey);
                    if (geminiResponse != null)
                    {
                        var products = await SearchProductsAsync(geminiResponse.SearchKeywords, geminiResponse.MaxBudget);
                        
                        return new ChatResponseDto
                        {
                            Message = geminiResponse.Reply,
                            Suggestions = geminiResponse.SuggestedQuestions ?? new List<string>(),
                            Products = products
                        };
                    }
                }

                // Fallback nếu không có API Key hoặc API lỗi
                return await GetFallbackResponseAsync(userMessage, user, categories);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi tư vấn Gemini: {ex.Message}");
                return new ChatResponseDto { Message = $"SYSTEM ERROR: {ex.Message}" };
            }
        }

        private async Task<GeminiParsedResponse?> CallGeminiApiAsync(string userMessage, ApplicationUser? user, List<string?> categories, string apiKey)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent?key={apiKey}";
            
            var userContext = user != null 
                ? $"Tên khách hàng: {user.FullName}. Đã mua {user.Orders?.Count ?? 0} đơn. Điểm tích lũy: {user.LoyaltyPoints?.Sum(lp => lp.Points) ?? 0}."
                : "Khách chưa đăng nhập.";

            var categoryContext = string.Join(", ", categories.Where(c => !string.IsNullOrEmpty(c)));

            var prompt = $@"
Bạn là trợ lý bán hàng ảo thân thiện của GAMINGSTORE.
Thông tin khách hàng: {userContext}
Danh mục cửa hàng: {categoryContext}
Tin nhắn của khách hàng: '{userMessage}'

Hãy phân tích tin nhắn và trả về một đối tượng JSON ĐÚNG chuẩn format sau, không thêm markdown ````json ở ngoài:
{{
  ""reply"": ""Câu trả lời giao tiếp tự nhiên của bạn (dùng được HTML cơ bản như <b>, <br>)"",
  ""searchKeywords"": [""từ khóa 1"", ""từ khóa 2""], 
  ""maxBudget"": 20000000, 
  ""suggestedQuestions"": [""Gợi ý câu hỏi tiếp theo 1"", ""Gợi ý 2""]
}}
Lưu ý:
- maxBudget là ngân sách tối đa khách đề cập (kiểu số nguyên). Nếu không đề cập thì để null.
- searchKeywords là các từ khóa tiếng Việt hoặc tiếng Anh quan trọng để tìm sản phẩm (ví dụ: laptop, asus, chuột không dây).
";

            var payload = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                },
                generationConfig = new { response_mime_type = "application/json" }
            };

            var response = await _httpClient.PostAsJsonAsync(url, payload);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Gemini API failed with status code {response.StatusCode}. Content: {errorContent}");
                throw new Exception($"API HTTP Error {response.StatusCode}: {errorContent}");
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(jsonString);
            
            try
            {
                var textContent = jsonDoc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (!string.IsNullOrEmpty(textContent))
                {
                    textContent = textContent.Trim();
                    if (textContent.StartsWith("```json"))
                        textContent = textContent.Substring(7);
                    if (textContent.StartsWith("```"))
                        textContent = textContent.Substring(3);
                    if (textContent.EndsWith("```"))
                        textContent = textContent.Substring(0, textContent.Length - 3);

                    textContent = textContent.Trim();

                    return JsonSerializer.Deserialize<GeminiParsedResponse>(textContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }
            catch(Exception ex)
            {
                throw new Exception($"Lỗi parse JSON: {ex.Message}. Raw JSON: {jsonString}");
            }

            throw new Exception($"Lỗi: textContent rỗng. Raw JSON: {jsonString}");
        }

        private async Task<List<Product>> SearchProductsAsync(List<string>? keywords, decimal? maxBudget)
        {
            var query = _context.Products
                .Include(p => p.Categories)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (maxBudget.HasValue && maxBudget.Value > 0)
            {
                query = query.Where(p => p.Price <= maxBudget.Value);
            }

            var products = await query.ToListAsync();

            if (keywords != null && keywords.Any())
            {
                products = products.Where(p => 
                    keywords.Any(k => 
                        (p.Name != null && p.Name.Contains(k, StringComparison.OrdinalIgnoreCase)) ||
                        (p.Description != null && p.Description.Contains(k, StringComparison.OrdinalIgnoreCase)) ||
                        (p.Categories != null && p.Categories.Any(c => c.Name != null && c.Name.Contains(k, StringComparison.OrdinalIgnoreCase)))
                    )
                )
                .OrderByDescending(p => p.ReviewCount)
                .Take(3)
                .ToList();
            }
            else
            {
                products = products.OrderByDescending(p => p.ReviewCount).Take(3).ToList();
            }

            return products;
        }

        private async Task<ChatResponseDto> GetFallbackResponseAsync(string userMessage, ApplicationUser? user, List<string?>? categories)
        {
            var msgLower = userMessage.ToLower();
            
            var products = await _context.Products.Where(p => p.IsActive).OrderByDescending(p => p.ReviewCount).Take(10).ToListAsync();
            var result = new ChatResponseDto();
            
            var name = user != null ? user.FullName : "bạn";
            
            if (msgLower.Contains("laptop") || msgLower.Contains("máy tính"))
            {
                result.Message = $"Xin chào {name}! 👋<br><br>Gợi ý một số laptop nổi bật:";
                result.Products = products.Where(p => p.Name != null && p.Name.ToLower().Contains("laptop")).Take(3).ToList();
                result.Suggestions = new List<string> { "Laptop dưới 20 triệu", "Laptop đồ họa" };
            }
            else if (msgLower.Contains("tai nghe") || msgLower.Contains("headphone"))
            {
                result.Message = $"Xin chào {name}! 👋<br><br>Tai nghe gaming được đánh giá cao nhất:";
                result.Products = products.Where(p => p.Name != null && p.Name.ToLower().Contains("tai nghe")).Take(3).ToList();
                result.Suggestions = new List<string> { "Tai nghe không dây", "Chuột gaming" };
            }
            else
            {
                result.Message = $"Xin chào {name}! 👋<br><br>Hiện tại bộ não AI của mình đang nghỉ ngơi, nhưng mình vẫn có thể gợi ý các sản phẩm hot nhất bên dưới cho bạn!";
                result.Products = products.Take(3).ToList();
                result.Suggestions = new List<string> { "Laptop gaming", "Bàn phím cơ", "Màn hình 144hz" };
            }

            return result;
        }
    }

    public class GeminiParsedResponse
    {
        public string Reply { get; set; } = string.Empty;
        public List<string> SearchKeywords { get; set; } = new();
        public decimal? MaxBudget { get; set; }
        public List<string> SuggestedQuestions { get; set; } = new();
    }
}
