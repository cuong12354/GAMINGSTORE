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

        public async Task<string> GetConsultationAsync(string userMessage, string userId)
        {
            try
            {
                // Lấy thông tin khách hàng từ database
                var user = await _context.Users
                    .Include(u => u.Orders)
                    .Include(u => u.LoyaltyPoints)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    return "Xin lỗi, không thể tìm thấy thông tin khách hàng.";

                // Lấy danh sách sản phẩm phổ biến
                var popularProducts = await _context.Products
                    .Where(p => p.IsActive)
                    .OrderByDescending(p => p.ReviewCount)
                    .Take(10)
                    .ToListAsync();

                // Lấy danh mục sản phẩm
                var categories = await _context.Categories
                    .Where(c => c.IsMenuVisible)
                    .Select(c => c.Name)
                    .ToListAsync();

                // Xây dựng response dựa vào dữ liệu database
                var response = BuildSmartResponse(userMessage, user, popularProducts, categories);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi tư vấn: {ex.Message}");
                return GetFallbackResponse(userMessage);
            }
        }

        private string BuildSmartResponse(string userMessage, ApplicationUser user, List<Product> products, List<string> categories)
        {
            var message = userMessage.ToLower();
            var totalOrders = user.Orders?.Count ?? 0;
            var totalSpent = user.Orders?.Sum(o => o.TotalPrice) ?? 0;
            var loyaltyPoints = user.LoyaltyPoints?.Sum(lp => lp.Points) ?? 0;

            // Tư vấn dựa vào từ khóa
            if (message.Contains("laptop") || message.Contains("máy tính"))
            {
                var laptops = products.Where(p => p.Name.ToLower().Contains("laptop")).ToList();
                if (laptops.Any())
                {
                    var cheapest = laptops.OrderBy(p => p.Price).First();
                    var mostRated = laptops.OrderByDescending(p => p.AverageRating).First();
                    return $"Xin chào {user.FullName}! 👋\n\n" +
                        $"Dựa vào lịch sử mua hàng của bạn ({totalOrders} đơn hàng, tổng chi tiêu: {totalSpent:C0}), tôi gợi ý:\n\n" +
                        $"💻 **Laptop rẻ nhất:** {cheapest.Name} - {cheapest.Price:C0}\n" +
                        $"⭐ **Laptop được đánh giá cao nhất:** {mostRated.Name} - {mostRated.AverageRating}/5 sao\n\n" +
                        $"Bạn hiện có {loyaltyPoints} điểm thưởng. Hãy liên hệ nhân viên để được tư vấn chi tiết!";
                }
            }

            if (message.Contains("tai nghe") || message.Contains("headphone"))
            {
                var headphones = products.Where(p => p.Name.ToLower().Contains("tai nghe") || p.Name.ToLower().Contains("headphone")).ToList();
                if (headphones.Any())
                {
                    var best = headphones.OrderByDescending(p => p.AverageRating).First();
                    return $"Xin chào {user.FullName}! 👋\n\n" +
                        $"Tai nghe gaming được đánh giá cao nhất: **{best.Name}** - {best.AverageRating}/5 sao\n" +
                        $"Giá: {best.Price:C0}\n\n" +
                        $"Bạn có {loyaltyPoints} điểm thưởng có thể dùng để giảm giá. Liên hệ nhân viên để biết thêm chi tiết!";
                }
            }

            if (message.Contains("màn hình") || message.Contains("monitor"))
            {
                var monitors = products.Where(p => p.Name.ToLower().Contains("màn hình") || p.Name.ToLower().Contains("monitor")).ToList();
                if (monitors.Any())
                {
                    var best = monitors.OrderByDescending(p => p.AverageRating).First();
                    return $"Xin chào {user.FullName}! 👋\n\n" +
                        $"Màn hình gaming được đánh giá cao nhất: **{best.Name}** - {best.AverageRating}/5 sao\n" +
                        $"Giá: {best.Price:C0}\n\n" +
                        $"Bạn là khách hàng thân thiết với {totalOrders} đơn hàng. Hãy liên hệ để được ưu đãi đặc biệt!";
                }
            }

            if (message.Contains("chuột") || message.Contains("mouse"))
            {
                var mice = products.Where(p => p.Name.ToLower().Contains("chuột") || p.Name.ToLower().Contains("mouse")).ToList();
                if (mice.Any())
                {
                    var best = mice.OrderByDescending(p => p.AverageRating).First();
                    return $"Xin chào {user.FullName}! 👋\n\n" +
                        $"Chuột gaming được đánh giá cao nhất: **{best.Name}** - {best.AverageRating}/5 sao\n" +
                        $"Giá: {best.Price:C0}\n\n" +
                        $"Liên hệ nhân viên để được tư vấn thêm!";
                }
            }

            if (message.Contains("bàn phím") || message.Contains("keyboard"))
            {
                var keyboards = products.Where(p => p.Name.ToLower().Contains("bàn phím") || p.Name.ToLower().Contains("keyboard")).ToList();
                if (keyboards.Any())
                {
                    var best = keyboards.OrderByDescending(p => p.AverageRating).First();
                    return $"Xin chào {user.FullName}! 👋\n\n" +
                        $"Bàn phím gaming được đánh giá cao nhất: **{best.Name}** - {best.AverageRating}/5 sao\n" +
                        $"Giá: {best.Price:C0}\n\n" +
                        $"Liên hệ nhân viên để được tư vấn thêm!";
                }
            }

            // Nếu không tìm thấy sản phẩm cụ thể, gợi ý sản phẩm phổ biến
            if (products.Any())
            {
                var topProduct = products.OrderByDescending(p => p.AverageRating).First();
                return $"Xin chào {user.FullName}! 👋\n\n" +
                    $"Sản phẩm được đánh giá cao nhất của chúng tôi: **{topProduct.Name}** - {topProduct.AverageRating}/5 sao\n" +
                    $"Giá: {topProduct.Price:C0}\n\n" +
                    $"Bạn có {loyaltyPoints} điểm thưởng. Liên hệ nhân viên để được tư vấn chi tiết!";
            }

            return GetFallbackResponse(userMessage);
        }

        private string GetFallbackResponse(string userMessage)
        {
            // Fallback response khi API không hoạt động
            if (userMessage.ToLower().Contains("laptop") || userMessage.ToLower().Contains("máy tính"))
                return "Xin lỗi, hiện tại tôi gặp sự cố kỹ thuật. Nhưng tôi có thể gợi ý: Chúng tôi có các laptop gaming từ các thương hiệu nổi tiếng như ASUS, MSI, Lenovo. Vui lòng liên hệ với nhân viên bán hàng để được tư vấn chi tiết.";
            
            if (userMessage.ToLower().Contains("tai nghe") || userMessage.ToLower().Contains("headphone"))
                return "Xin lỗi, hiện tại tôi gặp sự cố kỹ thuật. Nhưng tôi có thể gợi ý: Chúng tôi có tai nghe gaming chất lượng cao từ các thương hiệu như SteelSeries, HyperX, Corsair. Vui lòng liên hệ với nhân viên bán hàng để được tư vấn chi tiết.";
            
            if (userMessage.ToLower().Contains("màn hình") || userMessage.ToLower().Contains("monitor"))
                return "Xin lỗi, hiện tại tôi gặp sự cố kỹ thuật. Nhưng tôi có thể gợi ý: Chúng tôi có màn hình gaming 27 inch, 32 inch với tần số quét cao. Vui lòng liên hệ với nhân viên bán hàng để được tư vấn chi tiết.";
            
            return "Xin lỗi, hiện tại tôi gặp sự cố kỹ thuật. Vui lòng thử lại sau hoặc liên hệ với nhân viên bán hàng để được hỗ trợ.";
        }
    }
}
