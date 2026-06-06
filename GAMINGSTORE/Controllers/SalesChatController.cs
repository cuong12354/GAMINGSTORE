using System.Security.Claims;
using GAMINGSTORE.Models;
using GAMINGSTORE.Services;
using Microsoft.AspNetCore.Mvc;

namespace GAMINGSTORE.Controllers
{
    [Route("[controller]")]
    public class SalesChatController : Controller
    {
        private readonly IGeminiService _geminiService;

        public SalesChatController(IGeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] SalesChatRequest request)
        {
            var message = request?.Message?.Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                return Json(new SalesChatResponse
                {
                    Message = "Bạn cứ nhắn nhu cầu như: laptop gaming dưới 20 triệu, chuột không dây, bàn phím RGB hoặc sản phẩm theo danh mục bạn muốn.",
                    Suggestions = new List<string> { "Laptop gaming", "Màn hình", "Chuột gaming", "Dưới 20 triệu" }
                });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var geminiResponse = await _geminiService.GetConsultationAsync(message, userId);

            var productCards = geminiResponse.Products.Select(MapProduct).ToList();

            return Json(new SalesChatResponse
            {
                Message = geminiResponse.Message,
                Products = productCards,
                Suggestions = geminiResponse.Suggestions
            });
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