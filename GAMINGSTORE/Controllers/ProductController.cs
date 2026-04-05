using GAMINGSTORE.Models;
using GAMINGSTORE.Repositories;
using GAMINGSTORE.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GAMINGSTORE.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        // ================= DANH SÁCH =================
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        // ================= CREATE =================
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product, List<IFormFile> imageFiles, List<int> categoryIds)
        {
            // ĐÃ SỬA: Thêm dòng này để fix lỗi không lưu được ảnh
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                if (imageFiles != null && imageFiles.Count > 0)
                {
                    var uploadedUrls = await SaveImages(imageFiles);
                    product.ImageUrl = string.Join(";", uploadedUrls);
                }

                // Khởi tạo Categories nếu không có
                if (product.Categories == null)
                    product.Categories = new List<Category>();

                // Thêm categories được chọn
                if (categoryIds != null && categoryIds.Count > 0)
                {
                    var allCategories = await _categoryRepository.GetAllAsync();
                    foreach (var categoryId in categoryIds)
                    {
                        var category = allCategories.FirstOrDefault(c => c.Id == categoryId);
                        if (category != null)
                            product.Categories.Add(category);
                    }
                }

                await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name");
            return View(product);
        }

        // ================= DETAILS =================
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
                return NotFound();

            var allProducts = (await _productRepository.GetAllAsync()).ToList();
            var productCategoryIds = product.Categories.Select(c => c.Id).ToHashSet();
            var primaryCategory = product.Categories
                .Select(c => c.Name)
                .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? "Thiết bị công nghệ";

            var relatedProducts = allProducts
                .Where(candidate => candidate.Id != product.Id)
                .Where(candidate => candidate.Categories.Any(category => productCategoryIds.Contains(category.Id)))
                .OrderByDescending(candidate => candidate.Categories.Count(category => productCategoryIds.Contains(category.Id)))
                .ThenBy(candidate => Math.Abs(candidate.Price - product.Price))
                .Take(4)
                .ToList();

            var viewModel = new ProductDetailsViewModel
            {
                Product = product,
                ImageUrls = ParseImageUrls(product.ImageUrl),
                Highlights = BuildHighlights(product, primaryCategory),
                Specifications = BuildSpecifications(product, primaryCategory),
                RelatedProducts = relatedProducts,
                PrimaryCategory = primaryCategory
            };

            return View(viewModel);
        }

        // ================= EDIT =================
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
                return NotFound();

            var categories = await _categoryRepository.GetAllAsync();
            var selectedCategoryIds = product.Categories?.Select(c => c.Id).ToList() ?? new List<int>();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name", selectedCategoryIds);

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Product product, List<IFormFile> imageFiles, List<int> categoryIds)
        {
            ModelState.Remove("ImageUrl");

            if (id != product.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var existingProduct = await _productRepository.GetByIdAsync(id);

                if (existingProduct == null)
                    return NotFound();

                if (imageFiles != null && imageFiles.Count > 0)
                {
                    var uploadedUrls = await SaveImages(imageFiles);
                    existingProduct.ImageUrl = string.Join(";", uploadedUrls);
                }

                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;

                // Cập nhật categories
                existingProduct.Categories.Clear();
                if (categoryIds != null && categoryIds.Count > 0)
                {
                    var allCategories = await _categoryRepository.GetAllAsync();
                    foreach (var categoryId in categoryIds)
                    {
                        var category = allCategories.FirstOrDefault(c => c.Id == categoryId);
                        if (category != null)
                            existingProduct.Categories.Add(category);
                    }
                }

                await _productRepository.UpdateAsync(existingProduct);

                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryRepository.GetAllAsync();
            var selectedCategoryIds = product.Categories?.Select(c => c.Id).ToList() ?? new List<int>();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name", selectedCategoryIds);

            return View(product);
        }

        // ================= DELETE =================
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // ================= SAVE IMAGES =================
        private async Task<List<string>> SaveImages(List<IFormFile> images)
        {
            string folder = "products";
            var folderPath = Path.Combine("wwwroot/images", folder);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var imageUrls = new List<string>();

            foreach (var image in images)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                imageUrls.Add($"/images/{folder}/{fileName}");
            }

            return imageUrls;
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