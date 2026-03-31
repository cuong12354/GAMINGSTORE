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
        // ĐÃ SỬA: Thay IFormFile bằng List<IFormFile> imageFiles
        public async Task<IActionResult> Create(Product product, List<IFormFile> imageFiles)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem người dùng có chọn ảnh nào không
                if (imageFiles != null && imageFiles.Count > 0)
                {
                    // Lấy danh sách đường dẫn trả về
                    var uploadedUrls = await SaveImages(imageFiles, product.CategoryId);

                    // Nối các đường dẫn lại với nhau bằng dấu chấm phẩy (;) và lưu vào cột ImageUrl
                    product.ImageUrl = string.Join(";", uploadedUrls);
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
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name", product.CategoryId);

            return View(product);
        }

        [HttpPost]
        // ĐÃ SỬA: Thay IFormFile bằng List<IFormFile> imageFiles
        public async Task<IActionResult> Edit(int id, Product product, List<IFormFile> imageFiles)
        {
            ModelState.Remove("ImageUrl");

            if (id != product.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var existingProduct = await _productRepository.GetByIdAsync(id);

                if (existingProduct == null)
                    return NotFound();

                // Nếu người dùng CÓ CHỌN ảnh mới để upload
                if (imageFiles != null && imageFiles.Count > 0)
                {
                    var uploadedUrls = await SaveImages(imageFiles, product.CategoryId);
                    // Ghi đè ảnh cũ bằng danh sách ảnh mới
                    existingProduct.ImageUrl = string.Join(";", uploadedUrls);
                }
                // Nếu không chọn ảnh mới -> Giữ nguyên existingProduct.ImageUrl đã có sẵn trong DB

                // Update các dữ liệu khác
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;

                await _productRepository.UpdateAsync(existingProduct);

                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name", product.CategoryId);

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

        // ================= SAVE IMAGES (ĐÃ NÂNG CẤP LÊN NHIỀU ẢNH) =================
        private async Task<List<string>> SaveImages(List<IFormFile> images, int categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            string folder = category.Name.ToLower().Replace(" ", "");
            var folderPath = Path.Combine("wwwroot/images", folder);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var imageUrls = new List<string>();

            // Duyệt qua từng file ảnh được tải lên
            foreach (var image in images)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Thêm đường dẫn vào danh sách
                imageUrls.Add($"/images/{folder}/{fileName}");
            }

            // Trả về danh sách các đường dẫn
            return imageUrls;
        }
        // ================= TÌM KIẾM THEO DANH MỤC (TỪ KHÓA) =================
        public async Task<IActionResult> Category(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return RedirectToAction("Index", "Home");
            }

            var allProducts = await _productRepository.GetAllAsync();

            var filteredProducts = allProducts
                .Where(p => (p.Name != null && p.Name.ToLower().Contains(keyword.ToLower())) ||
                            (p.Description != null && p.Description.ToLower().Contains(keyword.ToLower())))
                .ToList();

            ViewBag.CurrentKeyword = keyword;

            return View(filteredProducts);
        }
    }
}