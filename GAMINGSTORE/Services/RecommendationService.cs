using GAMINGSTORE.Data;
using GAMINGSTORE.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GAMINGSTORE.Services
{
    public interface IRecommendationService
    {
        Task<List<Product>> GetRecommendedProductsAsync(int currentProductId, string? userId, int count = 6);
    }

    public class RecommendationService : IRecommendationService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RecommendationService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Gợi ý sản phẩm thông minh - kết hợp 3 yếu tố:
        /// 1. Sản phẩm cùng danh mục (50%)
        /// 2. Sản phẩm bán chạy nhất (30%)
        /// 3. Sản phẩm từ lịch sử mua hàng (20%)
        /// </summary>
        public async Task<List<Product>> GetRecommendedProductsAsync(int currentProductId, string? userId, int count = 6)
        {
            try
            {
                var currentProduct = await _context.Products
                    .Include(p => p.Categories)
                    .FirstOrDefaultAsync(p => p.Id == currentProductId);

                if (currentProduct == null)
                    return new List<Product>();

                var recommendations = new Dictionary<int, double>(); // ProductId -> Score

                // 1️⃣ SẢN PHẨM CÙNG DANH MỤC (50% trọng số)
                var sameCategory = await _context.Products
                    .Where(p => p.Id != currentProductId &&
                                p.Categories.Any(c => currentProduct.Categories.Select(cc => cc.Id).Contains(c.Id)))
                    .ToListAsync();

                foreach (var product in sameCategory)
                {
                    if (!recommendations.ContainsKey(product.Id))
                        recommendations[product.Id] = 0;
                    recommendations[product.Id] += 50; // 50% trọng số
                }

                // 2️⃣ SẢN PHẨM BÁN CHẠY NHẤT (30% trọng số)
                var topSelling = await _context.OrderDetails
                    .Where(od => od.Product!.Id != currentProductId)
                    .GroupBy(od => od.ProductId)
                    .OrderByDescending(g => g.Count())
                    .Take(count)
                    .Select(g => g.Key)
                    .ToListAsync();

                foreach (var productId in topSelling)
                {
                    if (!recommendations.ContainsKey(productId))
                        recommendations[productId] = 0;
                    recommendations[productId] += 30; // 30% trọng số
                }

                // 3️⃣ SẢN PHẨM TỪ LỊCH SỬ MUA HÀNG (20% trọng số) - nếu user logged in
                if (!string.IsNullOrEmpty(userId))
                {
                    var userOrderHistory = await _context.Orders
                        .Where(o => o.UserId == userId)
                        .Include(o => o.OrderDetails)
                        .SelectMany(o => o.OrderDetails)
                        .Where(od => od.Product!.Id != currentProductId)
                        .Select(od => od.ProductId)
                        .Distinct()
                        .ToListAsync();

                    // Lấy các sản phẩm liên quan từ danh mục của tất cả sản phẩm đã mua
                    var relatedToHistory = await _context.Products
                        .Where(p => p.Id != currentProductId &&
                                    p.Categories.Any(c => _context.OrderDetails
                                        .Where(od => userOrderHistory.Contains(od.ProductId))
                                        .SelectMany(od => od.Product!.Categories)
                                        .Select(c2 => c2.Id)
                                        .Contains(c.Id)))
                        .Select(p => p.Id)
                        .Distinct()
                        .ToListAsync();

                    foreach (var productId in relatedToHistory.Take(count))
                    {
                        if (!recommendations.ContainsKey(productId))
                            recommendations[productId] = 0;
                        recommendations[productId] += 20; // 20% trọng số
                    }
                }

                // Sắp xếp theo score và lấy top N sản phẩm
                var recommendedIds = recommendations
                    .OrderByDescending(x => x.Value)
                    .Take(count)
                    .Select(x => x.Key)
                    .ToList();

                var recommendedProducts = await _context.Products
                    .Where(p => recommendedIds.Contains(p.Id))
                    .Include(p => p.Categories)
                    .Include(p => p.Images)
                    .OrderBy(p => recommendedIds.IndexOf(p.Id))
                    .ToListAsync();

                return recommendedProducts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Recommendation error: {ex.Message}");
                // Fallback: trả về các sản phẩm cùng danh mục
                var fallback = await _context.Products
                    .Include(p => p.Categories)
                    .Where(p => p.Id != currentProductId)
                    .Take(count)
                    .ToListAsync();
                return fallback;
            }
        }
    }
}
