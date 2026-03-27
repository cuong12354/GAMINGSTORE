using GAMINGSTORE.Models;
using GAMINGSTORE.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GAMINGSTORE.Controllers
{
    public class HomeController : Controller
    {

        private readonly ICategoryRepository _categoryRepository;

        private readonly IProductRepository _productRepository;

        public HomeController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index(string searchString, int? categoryId)
        {
            // Lấy toàn bộ sản phẩm
            var products = await _productRepository.GetAllAsync();

            // ================= SEARCH =================
            if (!string.IsNullOrEmpty(searchString))
            {
                var keyword = RemoveDiacritics(searchString.ToLower());

                products = products.Where(p =>
                    !string.IsNullOrEmpty(p.Name) &&
                    RemoveDiacritics(p.Name.ToLower()).Contains(keyword)
                ).ToList();
            }

            // ================= FILTER CATEGORY =================
            if (categoryId.HasValue)
            {
                products = products
                    .Where(p => p.CategoryId == categoryId.Value)
                    .ToList();
            }
            // ================= VIEWBAG =================
            ViewBag.SelectedCategoryId = categoryId; // 👉 dùng để highlight menu
            ViewBag.SearchString = searchString;     // 👉 giữ lại text search
            return View(products);
        }

        // ================= REMOVE DẤU =================
        private string RemoveDiacritics(string text)
        {
            var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
            var chars = normalized.Where(c =>
                System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                != System.Globalization.UnicodeCategory.NonSpacingMark);

            return new string(chars.ToArray())
                .Normalize(System.Text.NormalizationForm.FormC);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}

