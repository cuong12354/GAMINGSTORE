using GAMINGSTORE.Models;
using GAMINGSTORE.Repositories;
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
    }
}