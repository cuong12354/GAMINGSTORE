using GAMINGSTORE.Authorization;
using GAMINGSTORE.Models;
using GAMINGSTORE.Repositories;
using GAMINGSTORE.Services;
using GAMINGSTORE.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace GAMINGSTORE.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IRecommendationService _recommendationService;
        private readonly IReviewRepository _reviewRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService;

        public ProductController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IRecommendationService recommendationService,
            IReviewRepository reviewRepository,
            UserManager<ApplicationUser> userManager,
            IAuditService auditService)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _recommendationService = recommendationService;
            _reviewRepository = reviewRepository;
            _userManager = userManager;
            _auditService = auditService;
        }

        // ================= DETAILS =================
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
                return NotFound();

            var primaryCategory = product.Categories
                .Select(c => c.Name)
                .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? "Thiết bị công nghệ";

            // ✨ Lấy user ID nếu đã đăng nhập
            var userId = _userManager.GetUserId(User);

            // ✨ Gợi ý sản phẩm thông minh - kết hợp 3 yếu tố
            var recommendedProducts = await _recommendationService.GetRecommendedProductsAsync(id, userId, count: 6);

            // ✨ Lấy đánh giá sản phẩm
            var reviews = (await _reviewRepository.GetByProductIdAsync(id)).ToList();

            var viewModel = new ProductDetailsViewModel
            {
                Product = product,
                ImageUrls = ParseImageUrls(product.ImageUrl),
                Highlights = BuildHighlights(product, primaryCategory),
                Specifications = BuildSpecifications(product, primaryCategory),
                RelatedProducts = recommendedProducts,
                PrimaryCategory = primaryCategory
            };

            ViewBag.Reviews = reviews ?? new List<ProductReview>();

            return View(viewModel);
        }

        // ================= TÌM KIẾM THEO DANH MỤC (ĐÃ FIX CHUẨN) =================
        public async Task<IActionResult> Category(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return RedirectToAction("Index", "Home");
            }

            var allProducts = await _productRepository.GetAllAsync();

            // Lọc ra sản phẩm thuộc Category khớp với từ khóa hoặc tên sản phẩm chứa từ khóa
            var filteredProducts = allProducts
                .Where(p => (p.Categories != null && p.Categories.Any(c => c.Name != null && c.Name.ToLower().Contains(keyword.ToLower()))) ||
                            (p.Name != null && p.Name.ToLower().Contains(keyword.ToLower())))
                .ToList();

            ViewBag.CurrentKeyword = keyword;

            return View(filteredProducts);
        }

        private static List<string> ParseImageUrls(string? imageUrl)
        {
            var imageUrls = string.IsNullOrWhiteSpace(imageUrl)
                ? new List<string>()
                : imageUrl
                    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList();

            if (imageUrls.Count == 0)
            {
                imageUrls.Add("/images/no-image.png");
            }

            return imageUrls;
        }

        private static List<string> BuildHighlights(Product product, string primaryCategory)
        {
            var priceSegment = GetPriceSegment(product.Price);
            var useCase = GetUseCase(primaryCategory);
            var categories = product.Categories.Any()
                ? string.Join(", ", product.Categories.Select(c => c.Name))
                : primaryCategory;
            var descriptionState = string.IsNullOrWhiteSpace(product.Description)
                ? "Đã sẵn sàng để cập nhật cấu hình và nội dung chi tiết sâu hơn."
                : "Đã có mô tả chi tiết để khách hàng xem nhanh trước khi thêm vào giỏ.";

            return new List<string>
            {
                $"Thuộc nhóm {primaryCategory}, phù hợp cho nhu cầu {useCase}.",
                $"Nằm trong phân khúc {priceSegment}, dễ cân đối ngân sách theo mức giá hiện tại.",
                $"Danh mục liên kết: {categories}.",
                descriptionState
            };
        }

        private static List<ProductSpecificationItem> BuildSpecifications(Product product, string primaryCategory)
        {
            return new List<ProductSpecificationItem>
            {
                new() { Label = "Mã sản phẩm", Value = $"GS-{product.Id:0000}" },
                new() { Label = "Danh mục chính", Value = primaryCategory },
                new() { Label = "Phân khúc giá", Value = GetPriceSegment(product.Price) },
                new() { Label = "Mức giá hiện tại", Value = product.Price.ToString("C0", new System.Globalization.CultureInfo("vi-VN")) },
                new() { Label = "Bộ ảnh sản phẩm", Value = $"{ParseImageUrls(product.ImageUrl).Count} ảnh" },
                new() { Label = "Nhóm nhu cầu", Value = GetUseCase(primaryCategory) },
                new() { Label = "Tình trạng catalog", Value = "Hiển thị online và sẵn sàng đặt hàng" }
            };
        }

        private static string GetPriceSegment(decimal price)
        {
            return price switch
            {
                < 1000000m => "Phổ thông",
                < 5000000m => "Tầm trung",
                < 15000000m => "Cận cao cấp",
                < 30000000m => "Cao cấp",
                _ => "Flagship"
            };
        }

        private static string GetUseCase(string categoryName)
        {
            return categoryName switch
            {
                "Laptop Gaming" or "PC Gaming" or "Màn hình Gaming" or "Chuột Gaming" or "Tai nghe Gaming" or "Bàn phím cơ" or "Ghế Gaming" or "Lót chuột" => "gaming, streaming và giải trí hiệu năng cao",
                "Laptop Văn phòng" or "PC Đồng bộ" or "Màn hình Văn phòng" => "học tập, văn phòng và làm việc hằng ngày",
                "MacBook" or "PC Đồ họa" or "Màn hình Đồ họa" => "sáng tạo nội dung, thiết kế và xử lý tác vụ chuyên sâu",
                "CPU" or "VGA" or "Mainboard" or "RAM" or "Ổ cứng SSD" => "nâng cấp hệ thống và tối ưu hiệu năng tổng thể",
                _ => "mua sắm công nghệ và nâng cấp góc máy"
            };
        }
    }
}