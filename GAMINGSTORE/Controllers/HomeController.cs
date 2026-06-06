using GAMINGSTORE.Models;
using GAMINGSTORE.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace GAMINGSTORE.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMemoryCache _cache;

        public HomeController(IProductRepository productRepository, ICategoryRepository categoryRepository, IMemoryCache cache)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _cache = cache;
        }

        public async Task<IActionResult> Index(
            string? searchString,
            string? productType,
            decimal? minPrice,
            decimal? maxPrice,
            int? categoryId,
            string sortBy = "newest",
            int page = 1)
        {
            int pageSize = 12;

            var categories = await _categoryRepository.GetAllAsync();

            // Không cache sản phẩm ở trang Home để khi Admin thêm/sửa sản phẩm thì search cập nhật ngay.
            var allProducts = await _productRepository.GetAllAsync() ?? new List<Product>();

            IEnumerable<Product> products = allProducts;

            // 1. Lọc theo nhóm menu trước: laptop, pc, monitor, component, gear...
            if (!string.IsNullOrWhiteSpace(productType))
            {
                products = ApplyProductTypeFilter(products, productType);
            }

            // 2. Lọc theo khoảng giá
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value);
            }

            // 3. Lọc theo từ khóa chi tiết
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                products = ApplySearchFilter(products, searchString);
            }

            // 4. Lọc theo categoryId nếu có
            if (categoryId.HasValue)
            {
                products = products.Where(p =>
                    p.Categories != null &&
                    p.Categories.Any(c => c.Id == categoryId.Value));
            }

            // 5. Sort
            products = sortBy switch
            {
                "price_asc" => products.OrderBy(p => p.Price),
                "price_desc" => products.OrderByDescending(p => p.Price),
                "name_asc" => products.OrderBy(p => p.Name),
                "name_desc" => products.OrderByDescending(p => p.Name),
                _ => products.OrderByDescending(p => p.Id)
            };

            // 6. Pagination
            var totalProduct = products.Count();
            var totalPage = (int)Math.Ceiling(totalProduct / (double)pageSize);

            if (page < 1)
                page = 1;

            if (page > totalPage && totalPage > 0)
                page = totalPage;

            var productList = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 7. ViewBag
            ViewBag.Categories = categories;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SearchString = searchString;
            ViewBag.ProductType = productType;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortBy = sortBy;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPage = totalPage;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalProduct = totalProduct;

            ViewBag.AllHomeProducts = allProducts;

            return View(productList);
        }

        // Hàm này có công dụng lọc sản phẩm tuyệt đối dựa trên thẻ productType từ Menu truyền xuống.
        // Thay vì tìm từ khóa trong toàn bộ tên và mô tả sản phẩm (dễ bị dính chéo như Laptop Gaming có chữ "PC" trong mô tả), 
        // ở đây chúng ta CHỈ tìm kiếm trong Tên của Danh Mục (c.Name).
        // Ví dụ: productType="pc" thì chỉ lấy các sản phẩm thuộc danh mục có chữ "pc", "máy tính bàn", v.v...
        private IEnumerable<Product> ApplyProductTypeFilter(IEnumerable<Product> products, string productType)
        {
            var type = NormalizeText(productType);

            return type switch
            {
                "laptop" => products.Where(p => p.Categories != null && p.Categories.Any(c => 
                    NormalizeText(c.Name).Contains("laptop") || NormalizeText(c.Name).Contains("macbook"))),

                "pc" => products.Where(p => p.Categories != null && p.Categories.Any(c => 
                    NormalizeText(c.Name).Contains("pc") || NormalizeText(c.Name).Contains("may tinh ban") || NormalizeText(c.Name).Contains("desktop") || NormalizeText(c.Name).Contains("may bo"))),

                "monitor" => products.Where(p => p.Categories != null && p.Categories.Any(c => 
                    NormalizeText(c.Name).Contains("man hinh") || NormalizeText(c.Name).Contains("monitor") || NormalizeText(c.Name).Contains("display"))),

                "component" => products.Where(p => p.Categories != null && p.Categories.Any(c => 
                    new[] { "linh kien", "cpu", "vga", "card do hoa", "mainboard", "ram", "ssd", "hdd", "psu", "nguon", "tan nhiet", "case" }
                    .Any(k => NormalizeText(c.Name).Contains(k)))),

                "gear" => products.Where(p => p.Categories != null && p.Categories.Any(c => 
                    new[] { "gaming gear", "chuot", "ban phim", "lot chuot", "tay cam", "tai nghe", "loa", "micro", "microphone", "webcam", "ghe", "ban gaming" }
                    .Any(k => NormalizeText(c.Name).Contains(k)))),

                _ => products
            };
        }

        private IEnumerable<Product> ApplySearchFilter(IEnumerable<Product> products, string searchString)
        {
            var keyword = NormalizeText(searchString);

            if (keyword.Contains("rtx 4000"))
            {
                return products.Where(p => ProductContainsAny(p,
                    "rtx 4050", "rtx 4060", "rtx 4070", "rtx 4080", "rtx 4090"));
            }

            if (keyword.Contains("rtx 5000"))
            {
                return products.Where(p => ProductContainsAny(p,
                    "rtx 5050", "rtx 5060", "rtx 5070", "rtx 5080", "rtx 5090"));
            }

            if (keyword.Contains("core i5") && keyword.Contains("rtx 4060"))
            {
                return products.Where(p => ProductContainsAll(p, "core i5", "rtx 4060"));
            }

            if (keyword.Contains("core i7") && keyword.Contains("rtx 4070"))
            {
                return products.Where(p => ProductContainsAll(p, "core i7", "rtx 4070"));
            }

            if (keyword.Contains("core i9") && keyword.Contains("rtx 4090"))
            {
                return products.Where(p => ProductContainsAll(p, "core i9", "rtx 4090"));
            }

            if (keyword.Contains("ryzen 7") && keyword.Contains("rx 7000"))
            {
                return products.Where(p => ProductContainsAll(p, "ryzen 7", "rx 7000"));
            }

            // Tách các từ trong từ khóa tìm kiếm (bỏ các từ quá ngắn < 2 ký tự)
            var searchWords = keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                     .Where(w => w.Length >= 2)
                                     .ToList();

            if (searchWords.Any())
            {
                // Yêu cầu sản phẩm phải chứa tất cả các từ trong từ khóa tìm kiếm (phép AND)
                return products.Where(p => searchWords.All(word => ProductContains(p, word)));
            }

            return products;
        }

        private List<string> BuildSearchTerms(string keyword)
        {
            var terms = new List<string>();

            if (!string.IsNullOrWhiteSpace(keyword))
                terms.Add(keyword);

            var knownTerms = new[]
            {
                "msi", "asus", "rog", "lenovo", "legion", "acer", "predator",
                "gigabyte", "aorus", "apple", "macbook", "samsung", "dell", "lg",

                "intel", "core ultra", "core i5", "core i7", "core i9",
                "amd", "ryzen", "ryzen ai", "rtx", "rtx 4060", "rtx 4070", "rtx 4090",

                "gaming", "ai", "hi-end", "van phong", "do hoa", "build pc",

                "24 inch", "27 inch", "32 inch", "sieu rong", "144hz", "165hz",
                "240hz", "360hz", "oled",

                "cpu", "vga", "card do hoa", "mainboard", "ram", "ssd", "hdd",
                "psu", "nguon", "tan nhiet", "case", "quat",

                "chuot", "ban phim", "lot chuot", "tay cam", "tai nghe",
                "loa", "micro", "microphone", "webcam", "ghe", "ban gaming"
            };

            foreach (var term in knownTerms)
            {
                if (keyword.Contains(term) && !terms.Contains(term))
                    terms.Add(term);
            }

            // Tách thêm từng từ để search dễ ra hơn
            var splitTerms = keyword
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(x => x.Length >= 2);

            foreach (var term in splitTerms)
            {
                if (!terms.Contains(term))
                    terms.Add(term);
            }

            return terms.Distinct().ToList();
        }

        private bool ProductContains(Product product, string keyword)
        {
            keyword = NormalizeText(keyword);

            var name = NormalizeText(product.Name);
            var description = NormalizeText(product.Description);

            var matchName = name.Contains(keyword);
            var matchDescription = description.Contains(keyword);

            var matchCategory = product.Categories != null &&
                product.Categories.Any(c => NormalizeText(c.Name).Contains(keyword));

            return matchName || matchDescription || matchCategory;
        }

        private bool ProductContainsAny(Product product, params string[] keywords)
        {
            return keywords.Any(keyword => ProductContains(product, keyword));
        }

        private bool ProductContainsAll(Product product, params string[] keywords)
        {
            return keywords.All(keyword => ProductContains(product, keyword));
        }

        private string NormalizeText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            text = text.ToLower().Trim();

            var normalized = text.Normalize(NormalizationForm.FormD);

            var chars = normalized.Where(c =>
                CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);

            return new string(chars.ToArray())
                .Normalize(NormalizationForm.FormC);
        }

        public IActionResult Services()
        {
            return View();
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
