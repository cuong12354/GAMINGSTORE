using GAMINGSTORE.Authorization;
using GAMINGSTORE.Models;
using GAMINGSTORE.Repositories;
using GAMINGSTORE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace GAMINGSTORE.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = PermissionConstants.ProductManage)]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService;
        private readonly IMemoryCache _cache;

        public ProductController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            UserManager<ApplicationUser> userManager,
            IAuditService auditService,
            IMemoryCache cache)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _userManager = userManager;
            _auditService = auditService;
            _cache = cache;
        }

        // ================= DANH SÁCH =================
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        // ================= DETAILS (Optional, redirects to storefront public Details) =================
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            // Redirect to the storefront details page for public view consistency
            return RedirectToAction("Details", "Product", new { area = "", id = id });
        }

        // ================= CREATE =================
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product, List<IFormFile> imageFiles, List<int> categoryIds, int stockQuantity = 10)
        {
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

                // ✨ Gán Số lượng tồn kho ban đầu
                if (product.Inventory == null)
                {
                    product.Inventory = new Inventory
                    {
                        StockQuantity = stockQuantity,
                        MinimumStockLevel = 10,
                        LastRestockDate = DateTime.UtcNow
                    };
                }

                await _productRepository.AddAsync(product);

                // 📋 Xóa cache sản phẩm
                _cache.Remove("AllProducts");

                // 📋 Log audit
                var userId = _userManager.GetUserId(User);
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = Request.Headers["User-Agent"].ToString();
                await _auditService.LogActionAsync(userId, "Create", "Product", product.Id,
                    $"Tạo sản phẩm: {product.Name}", null,
                    JsonSerializer.Serialize(product),
                    ipAddress, userAgent);

                TempData["Success"] = $"✅ Đã thêm sản phẩm {product.Name} thành công.";
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name");
            return View(product);
        }

        // ================= EDIT =================
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
                return NotFound();

            var categories = await _categoryRepository.GetAllAsync();
            var selectedCategoryIds = product.Categories?.Select(c => c.Id).ToList() ?? new List<int>();
            ViewBag.CategoryList = new MultiSelectList(categories, "Id", "Name", selectedCategoryIds);

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Product product, List<IFormFile> imageFiles, List<int> categoryIds, int stockQuantity = 0)
        {
            ModelState.Remove("ImageUrl");

            if (id != product.Id)
                return NotFound();

            // 📋 Lấy sản phẩm cũ để ghi log thay đổi
            var oldProduct = await _productRepository.GetByIdAsync(id);
            if (oldProduct == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                if (imageFiles != null && imageFiles.Count > 0)
                {
                    var uploadedUrls = await SaveImages(imageFiles);
                    oldProduct.ImageUrl = string.Join(";", uploadedUrls);
                }

                oldProduct.Name = product.Name;
                oldProduct.Price = product.Price;
                oldProduct.Description = product.Description;

                // Cập nhật categories
                oldProduct.Categories.Clear();
                if (categoryIds != null && categoryIds.Count > 0)
                {
                    var allCategories = await _categoryRepository.GetAllAsync();
                    foreach (var categoryId in categoryIds)
                    {
                        var category = allCategories.FirstOrDefault(c => c.Id == categoryId);
                        if (category != null)
                            oldProduct.Categories.Add(category);
                    }
                }

                // ✨ Cập nhật Tồn Kho
                if (oldProduct.Inventory == null)
                {
                    oldProduct.Inventory = new Inventory
                    {
                        StockQuantity = stockQuantity,
                        MinimumStockLevel = 10,
                        LastRestockDate = DateTime.UtcNow
                    };
                }
                else
                {
                    oldProduct.Inventory.StockQuantity = stockQuantity;
                    oldProduct.Inventory.LastRestockDate = DateTime.UtcNow;
                }

                await _productRepository.UpdateAsync(oldProduct);

                // 📋 Xóa cache sản phẩm
                _cache.Remove("AllProducts");

                // 📋 Log audit
                var userId = _userManager.GetUserId(User);
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = Request.Headers["User-Agent"].ToString();
                await _auditService.LogActionAsync(userId, "Update", "Product", id,
                    $"Cập nhật sản phẩm: {oldProduct.Name}",
                    JsonSerializer.Serialize(oldProduct),
                    JsonSerializer.Serialize(oldProduct), // We serialized the updated object
                    ipAddress, userAgent);

                TempData["Success"] = $"✅ Đã cập nhật sản phẩm {oldProduct.Name} thành công.";
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
            // 📋 Lấy sản phẩm trước khi xóa để ghi log
            var product = await _productRepository.GetByIdAsync(id);

            await _productRepository.DeleteAsync(id);

            // 📋 Xóa cache sản phẩm
            _cache.Remove("AllProducts");

            // 📋 Log audit
            if (product != null)
            {
                var userId = _userManager.GetUserId(User);
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = Request.Headers["User-Agent"].ToString();
                await _auditService.LogActionAsync(userId, "Delete", "Product", id,
                    $"Xóa sản phẩm: {product.Name}",
                    JsonSerializer.Serialize(product),
                    null, ipAddress, userAgent);
            }

            TempData["Success"] = $"✅ Đã xóa sản phẩm thành công.";
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
    }
}