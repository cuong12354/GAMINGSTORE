using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using GAMINGSTORE.Models;
using GAMINGSTORE.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GAMINGSTORE.Controllers
{
    [Route("[controller]")]
    public class SalesChatController : Controller
    {
        private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "toi", "muon", "tim", "san", "pham", "shop", "cua", "hang", "co", "khong",
            "gi", "nao", "de", "giup", "tu", "van", "cho", "minh", "voi", "la", "mot",
            "nhung", "cac", "gaming", "store", "giong", "nhe", "a", "ah", "em", "anh"
        };

        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public SalesChatController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] SalesChatRequest request)
        {
            var message = request?.Message?.Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                return Json(CreateFallbackResponse("Bạn cứ nhắn nhu cầu như: laptop gaming dưới 20 triệu, chuột không dây, bàn phím RGB hoặc sản phẩm theo danh mục bạn muốn."));
            }

            var normalizedMessage = Normalize(message);
            var products = (await _productRepository.GetAllAsync()).ToList();
            var categories = (await _categoryRepository.GetAllAsync()).ToList();

            if (ContainsAny(normalizedMessage, "xin chao", "chao", "hello", "hi", "alo"))
            {
                return Json(new SalesChatResponse
                {
                    Message = "Chào bạn, mình là trợ lý bán hàng của GAMINGSTORE. Bạn có thể hỏi theo nhu cầu, khoảng giá hoặc danh mục, ví dụ: laptop gaming dưới 25 triệu, tai nghe chơi game, màn hình 27 inch.",
                    Suggestions = GetDefaultSuggestions(categories)
                });
            }

            if (ContainsAny(normalizedMessage, "danh muc", "co nhung loai nao", "ban gi", "shop co gi"))
            {
                var categoryNames = categories.Select(c => c.Name ?? string.Empty).Where(name => !string.IsNullOrWhiteSpace(name)).Take(8).ToList();
                var categoryText = categoryNames.Any() ? string.Join(", ", categoryNames) : "sản phẩm gaming";

                return Json(new SalesChatResponse
                {
                    Message = $"Hiện shop đang có các nhóm nổi bật như: {categoryText}. Bạn muốn mình gợi ý theo nhóm nào hoặc theo tầm giá bao nhiêu?",
                    Suggestions = categoryNames.Take(4).Concat(new[] { "Dưới 20 triệu", "Sản phẩm bán chạy" }).ToList()
                });
            }

            if (ContainsAny(normalizedMessage, "ship", "giao hang", "van chuyen"))
            {
                return Json(CreateFallbackResponse("Shop hỗ trợ quy trình đặt hàng trực tiếp trên website. Bạn chọn sản phẩm, thêm vào giỏ, nhập địa chỉ giao hàng ở bước checkout và hệ thống sẽ lưu đơn để xử lý.", categories));
            }

            if (ContainsAny(normalizedMessage, "thanh toan", "payment", "tra truoc", "chuyen khoan", "cod"))
            {
                return Json(CreateFallbackResponse("Bạn có thể vào giỏ hàng và chọn phương thức thanh toán tại trang checkout. Nếu cần, mình có thể gợi ý trước một vài sản phẩm phù hợp với nhu cầu của bạn.", categories));
            }

            var budget = ExtractBudget(normalizedMessage);
            var matchedProducts = ScoreProducts(products, normalizedMessage, budget);

            if (budget.HasValue && !matchedProducts.Any())
            {
                matchedProducts = products
                    .Where(p => p.Price <= budget.Value)
                    .OrderByDescending(p => p.Price)
                    .Take(3)
                    .ToList();
            }

            if (!matchedProducts.Any() && ContainsAny(normalizedMessage, "ban chay", "goi y", "de cu", "tot nhat", "noi bat"))
            {
                matchedProducts = products.OrderByDescending(p => p.Price).Take(3).ToList();
            }

            if (!matchedProducts.Any())
            {
                return Json(CreateFallbackResponse("Mình chưa bắt đúng ý lắm. Bạn thử nói rõ hơn theo dạng: tên sản phẩm, danh mục hoặc khoảng giá, ví dụ laptop gaming dưới 20 triệu, tai nghe RGB, bàn phím cơ.", categories));
            }

            var productCards = matchedProducts.Take(3).Select(MapProduct).ToList();
            var intro = budget.HasValue
                ? $"Mình tìm được vài sản phẩm phù hợp trong tầm {budget.Value.ToString("N0", new CultureInfo("vi-VN"))} đ:"
                : "Đây là vài sản phẩm phù hợp với nhu cầu bạn đang hỏi:";

            return Json(new SalesChatResponse
            {
                Message = intro,
                Products = productCards,
                Suggestions = BuildFollowUpSuggestions(productCards, categories)
            });
        }

        private static List<Product> ScoreProducts(IEnumerable<Product> products, string normalizedMessage, decimal? budget)
        {
            var keywords = normalizedMessage
                .Split(new[] { ' ', ',', '.', ';', ':', '-', '_', '/', '\\' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(token => token.Length > 1 && !StopWords.Contains(token))
                .Distinct()
                .ToList();

            var scoredProducts = products
                .Select(product => new
                {
                    Product = product,
                    Score = CalculateScore(product, normalizedMessage, keywords, budget)
                })
                .Where(item => item.Score > 0)
                .OrderByDescending(item => item.Score)
                .ThenBy(item => item.Product.Price)
                .Select(item => item.Product)
                .ToList();

            return scoredProducts;
        }

        private static int CalculateScore(Product product, string normalizedMessage, List<string> keywords, decimal? budget)
        {
            var score = 0;
            var productName = Normalize(product.Name);
            var description = Normalize(product.Description);
            var categoryNames = product.Categories?.Select(c => Normalize(c.Name)).Where(name => !string.IsNullOrWhiteSpace(name)).ToList() ?? new List<string>();

            foreach (var keyword in keywords)
            {
                if (productName.Contains(keyword))
                {
                    score += 5;
                }

                if (description.Contains(keyword))
                {
                    score += 2;
                }

                if (categoryNames.Any(name => name.Contains(keyword)))
                {
                    score += 4;
                }
            }

            if (normalizedMessage.Contains("duoi") || normalizedMessage.Contains("tam") || normalizedMessage.Contains("khoang gia"))
            {
                if (budget.HasValue && product.Price <= budget.Value)
                {
                    score += 3;
                }
            }

            if (ContainsAny(normalizedMessage, "re", "gia mem", "gia tot", "tiet kiem") && product.Price <= 20000000)
            {
                score += 2;
            }

            return score;
        }

        private static decimal? ExtractBudget(string normalizedMessage)
        {
            var match = Regex.Match(normalizedMessage, @"(\d+[\.,]?\d*)\s*(tr|trieu|m|k|nghin|ngan|d)");
            if (!match.Success)
            {
                return null;
            }

            var numberText = match.Groups[1].Value.Replace(",", ".");
            if (!decimal.TryParse(numberText, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
            {
                return null;
            }

            var unit = match.Groups[2].Value;
            return unit switch
            {
                "tr" or "trieu" or "m" => amount * 1000000,
                "k" or "nghin" or "ngan" => amount * 1000,
                _ => amount
            };
        }

        private static SalesChatResponse CreateFallbackResponse(string message, IEnumerable<Category>? categories = null)
        {
            return new SalesChatResponse
            {
                Message = message,
                Suggestions = categories?.Select(c => c.Name ?? string.Empty).Where(name => !string.IsNullOrWhiteSpace(name)).Take(4).ToList()
                    ?? new List<string> { "Laptop gaming", "Màn hình", "Chuột gaming", "Dưới 20 triệu" }
            };
        }

        private static List<string> GetDefaultSuggestions(IEnumerable<Category> categories)
        {
            var suggestions = categories
                .Select(c => c.Name ?? string.Empty)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Take(3)
                .ToList();

            suggestions.Add("Laptop gaming dưới 20 triệu");
            suggestions.Add("Sản phẩm bán chạy");
            return suggestions;
        }

        private static List<string> BuildFollowUpSuggestions(IEnumerable<SalesChatProductCard> products, IEnumerable<Category> categories)
        {
            var productCategories = products
                .SelectMany(p => p.Categories)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct()
                .Take(2)
                .ToList();

            var fallbackCategories = categories
                .Select(c => c.Name ?? string.Empty)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct()
                .Take(2);

            return productCategories
                .Concat(fallbackCategories)
                .Concat(new[] { "Cho mình sản phẩm rẻ hơn", "Hướng dẫn đặt hàng" })
                .Distinct()
                .Take(4)
                .ToList();
        }

        private static SalesChatProductCard MapProduct(Product product)
        {
            var primaryImage = product.ImageUrl?.Split(';', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "/images/no-image.png";

            return new SalesChatProductCard
            {
                Id = product.Id,
                Name = product.Name ?? "Sản phẩm",
                Price = product.Price,
                ImageUrl = primaryImage,
                Categories = product.Categories?.Select(c => c.Name ?? string.Empty).Where(name => !string.IsNullOrWhiteSpace(name)).Take(2).ToList() ?? new List<string>(),
                Url = $"/Product/Details/{product.Id}"
            };
        }

        private static bool ContainsAny(string value, params string[] keywords)
        {
            return keywords.Any(value.Contains);
        }

        private static string Normalize(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var normalized = text.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();

            foreach (var character in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(character);
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC).Replace('đ', 'd');
        }

        public class SalesChatRequest
        {
            public string? Message { get; set; }
        }

        public class SalesChatResponse
        {
            public string Message { get; set; } = string.Empty;
            public List<string> Suggestions { get; set; } = new();
            public List<SalesChatProductCard> Products { get; set; } = new();
        }

        public class SalesChatProductCard
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public string ImageUrl { get; set; } = string.Empty;
            public string Url { get; set; } = string.Empty;
            public List<string> Categories { get; set; } = new();
        }
    }
}