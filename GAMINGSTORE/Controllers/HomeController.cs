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

        public async Task<IActionResult> Index(string searchString, int? categoryId, string sortBy = "newest", int page = 1)
        {
            int pageSize = 12;

            // Lấy toàn bộ sản phẩm
            var products = (await _productRepository.GetAllAsync()).AsQueryable();
            var categories = await _categoryRepository.GetAllAsync();

            // ================= SEARCH =================
            if (!string.IsNullOrEmpty(searchString))
            {
                var keyword = RemoveDiacritics(searchString.ToLower());

                products = products.Where(p =>
                    !string.IsNullOrEmpty(p.Name) &&
                    RemoveDiacritics(p.Name.ToLower()).Contains(keyword)
                );
            }

            // ================= FILTER CATEGORY =================
            if (categoryId.HasValue)
            {
                products = products
                    .Where(p => p.Categories != null && p.Categories.Any(c => c.Id == categoryId.Value));
            }

            // ================= SORTING =================
            products = sortBy switch
            {
                "price_asc" => products.OrderBy(p => p.Price),
                "price_desc" => products.OrderByDescending(p => p.Price),
                "name_asc" => products.OrderBy(p => p.Name),
                "name_desc" => products.OrderByDescending(p => p.Name),
                _ => products.OrderByDescending(p => p.Id) // newest by default
            };

            // ================= PAGINATION =================
            var totalProduct = products.Count();
            var totalPage = (int)Math.Ceiling(totalProduct / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPage && totalPage > 0) page = totalPage;

            var productList = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // ================= VIEWBAG =================
            ViewBag.Categories = categories;         // 👉 danh sách tất cả categories
            ViewBag.SelectedCategoryId = categoryId;  // 👉 dùng để highlight menu
            ViewBag.SearchString = searchString;      // 👉 giữ lại text search
            ViewBag.SortBy = sortBy;                  // 👉 giữ lại sort option
            ViewBag.CurrentPage = page;               // 👉 trang hiện tại
            ViewBag.TotalPage = totalPage;            // 👉 tổng số trang
            ViewBag.PageSize = pageSize;              // 👉 số sản phẩm mỗi trang
            ViewBag.TotalProduct = totalProduct;      // 👉 tổng số sản phẩm

            return View(productList);
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

