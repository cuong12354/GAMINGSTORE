using GAMINGSTORE.Models;
using GAMINGSTORE.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;

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

            // Sử dụng IQueryable để lọc dưới SQL Server thay vì kéo toàn bộ về RAM
            IQueryable<Product> query = _productRepository.GetQueryable();

            // 1. Lọc theo nhóm menu trước
            if (!string.IsNullOrWhiteSpace(productType))
            {
                query = ApplyProductTypeFilter(query, productType);
            }

            // 2. Lọc theo khoảng giá
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // 3. Lọc theo từ khóa chi tiết (AND logic)
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = ApplySearchFilter(query, searchString);
            }

            // 4. Lọc theo categoryId nếu có
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.Categories.Any(c => c.Id == categoryId.Value));
            }

            // 5. Sort
            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name_asc" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                _ => query.OrderByDescending(p => p.Id)
            };

            // 6. Pagination & Execute Query
            var totalProduct = await query.CountAsync(); // Chỉ đếm dưới DB
            var totalPage = (int)Math.Ceiling(totalProduct / (double)pageSize);

            if (page < 1)
                page = 1;

            if (page > totalPage && totalPage > 0)
                page = totalPage;

            // Truy vấn lấy đúng 12 sản phẩm
            var productList = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

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

            var allProducts = await _productRepository.GetAllAsync() ?? new List<Product>();
            ViewBag.AllHomeProducts = allProducts;

            return View(productList);
        }

        // Hàm này có công dụng lọc sản phẩm tuyệt đối dựa trên thẻ productType từ Menu truyền xuống.
        // Thay vì tìm từ khóa trong toàn bộ tên và mô tả sản phẩm (dễ bị dính chéo như Laptop Gaming có chữ "PC" trong mô tả), 
        // ở đây chúng ta CHỈ tìm kiếm trong Tên của Danh Mục (c.Name).
        // Ví dụ: productType="pc" thì chỉ lấy các sản phẩm thuộc danh mục có chữ "pc", "máy tính bàn", v.v...
        private IQueryable<Product> ApplyProductTypeFilter(IQueryable<Product> query, string productType)
        {
            var type = productType.ToLower().Trim();

            return type switch
            {
                "laptop" => query.Where(p => p.Categories.Any(c => 
                    c.Name != null && (c.Name.Contains("laptop") || c.Name.Contains("macbook")))),

                "pc" => query.Where(p => p.Categories.Any(c => 
                    c.Name != null && (c.Name.Contains("pc") || c.Name.Contains("máy tính bàn") || c.Name.Contains("desktop") || c.Name.Contains("máy bộ")))),

                "monitor" => query.Where(p => p.Categories.Any(c => 
                    c.Name != null && (c.Name.Contains("màn hình") || c.Name.Contains("monitor") || c.Name.Contains("display")))),

                "component" => query.Where(p => p.Categories.Any(c => 
                    c.Name != null && (c.Name.Contains("linh kiện") || c.Name.Contains("cpu") || c.Name.Contains("vga") || c.Name.Contains("card đồ họa") || c.Name.Contains("mainboard") || c.Name.Contains("ram") || c.Name.Contains("ssd") || c.Name.Contains("hdd") || c.Name.Contains("psu") || c.Name.Contains("nguồn") || c.Name.Contains("tản nhiệt") || c.Name.Contains("case")))),

                "gear" => query.Where(p => p.Categories.Any(c => 
                    c.Name != null && (c.Name.Contains("gaming gear") || c.Name.Contains("chuột") || c.Name.Contains("bàn phím") || c.Name.Contains("lót chuột") || c.Name.Contains("tay cầm") || c.Name.Contains("tai nghe") || c.Name.Contains("loa") || c.Name.Contains("micro") || c.Name.Contains("microphone") || c.Name.Contains("webcam") || c.Name.Contains("ghế") || c.Name.Contains("bàn gaming")))),

                _ => query
            };
        }

        private IQueryable<Product> ApplySearchFilter(IQueryable<Product> query, string searchString)
        {
            var keyword = searchString.ToLower().Trim();

            // Hardcoded special cases
            if (keyword.Contains("rtx 4000"))
            {
                return query.Where(p => p.Name != null && (p.Name.Contains("rtx 4050") || p.Name.Contains("rtx 4060") || p.Name.Contains("rtx 4070") || p.Name.Contains("rtx 4080") || p.Name.Contains("rtx 4090")));
            }
            if (keyword.Contains("rtx 5000"))
            {
                return query.Where(p => p.Name != null && (p.Name.Contains("rtx 5050") || p.Name.Contains("rtx 5060") || p.Name.Contains("rtx 5070") || p.Name.Contains("rtx 5080") || p.Name.Contains("rtx 5090")));
            }
            if (keyword.Contains("core i5") && keyword.Contains("rtx 4060"))
            {
                return query.Where(p => p.Name != null && p.Name.Contains("core i5") && p.Name.Contains("rtx 4060"));
            }
            if (keyword.Contains("core i7") && keyword.Contains("rtx 4070"))
            {
                return query.Where(p => p.Name != null && p.Name.Contains("core i7") && p.Name.Contains("rtx 4070"));
            }
            if (keyword.Contains("core i9") && keyword.Contains("rtx 4090"))
            {
                return query.Where(p => p.Name != null && p.Name.Contains("core i9") && p.Name.Contains("rtx 4090"));
            }
            if (keyword.Contains("ryzen 7") && keyword.Contains("rx 7000"))
            {
                return query.Where(p => p.Name != null && p.Name.Contains("ryzen 7") && p.Name.Contains("rx 7000"));
            }
            if (keyword == "144hz 165hz")
            {
                return query.Where(p => p.Name != null && (p.Name.Contains("144hz") || p.Name.Contains("165hz")));
            }
            if (keyword == "240hz 360hz")
            {
                return query.Where(p => p.Name != null && (p.Name.Contains("240hz") || p.Name.Contains("360hz")));
            }
            if (keyword == "ssd hdd")
            {
                return query.Where(p => (p.Name != null && (p.Name.Contains("ssd") || p.Name.Contains("hdd"))) || 
                                        p.Categories.Any(c => c.Name != null && (c.Name.Contains("ssd") || c.Name.Contains("hdd"))));
            }

            var terms = BuildSearchTerms(searchString);

            if (terms.Any())
            {
                // LOGIC AND: Sản phẩm phải chứa TẤT CẢ các từ khóa
                foreach (var term in terms)
                {
                    var t = term; // Local copy for LINQ expression
                    query = query.Where(p => 
                        (p.Name != null && p.Name.Contains(t)) ||
                        (p.Description != null && p.Description.Contains(t)) ||
                        p.Categories.Any(c => c.Name != null && c.Name.Contains(t))
                    );
                }
            }

            return query;
        }

        private List<string> BuildSearchTerms(string keyword)
        {
            var terms = new List<string>();

            // Các từ khóa thông dụng cần giữ nguyên khối
            var knownTerms = new[]
            {
                "msi", "asus", "rog", "lenovo", "legion", "acer", "predator",
                "gigabyte", "aorus", "apple", "macbook", "samsung", "dell", "lg",
                "intel", "core ultra", "core i5", "core i7", "core i9",
                "amd", "ryzen", "ryzen ai", "rtx 4060", "rtx 4070", "rtx 4090", "rtx",
                "gaming", "ai", "hi-end", "văn phòng", "đồ họa", "build pc",
                "24 inch", "27 inch", "32 inch", "144hz", "165hz", "240hz", "360hz", "oled",
                "chuột", "bàn phím", "tai nghe", "màn hình", "laptop", "pc"
            };

            var lowerKeyword = keyword.ToLower();

            foreach (var term in knownTerms)
            {
                if (lowerKeyword.Contains(term))
                {
                    terms.Add(term);
                    lowerKeyword = lowerKeyword.Replace(term, "").Trim();
                }
            }

            // Tách các từ còn lại theo dấu cách
            var splitTerms = lowerKeyword
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(x => x.Length >= 2);

            foreach (var term in splitTerms)
            {
                if (!terms.Contains(term))
                    terms.Add(term);
            }

            return terms.Distinct().ToList();
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
