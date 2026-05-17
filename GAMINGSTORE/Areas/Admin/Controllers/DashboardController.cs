using GAMINGSTORE.Authorization;
using GAMINGSTORE.Data;
using GAMINGSTORE.Models;
using GAMINGSTORE.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GAMINGSTORE.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = PermissionConstants.DashboardAccess)]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public DashboardController(ApplicationDbContext context, IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _context = context;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        // GET: Admin/Dashboard/Index
        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;
            var thisMonth = DateTime.UtcNow.AddMonths(-1);

            // Orders Data
            var totalOrders = _context.Orders.Count();
            var pendingOrders = _context.Orders.Count(o => o.Status == "Pending");
            var todayOrders = _context.Orders.Where(o => o.OrderDate.Date == today).Count();
            var monthlyOrders = _context.Orders.Where(o => o.OrderDate >= thisMonth).Count();

            // Revenue Data
            var totalRevenue = _context.Orders.Sum(o => o.TotalPrice);
            var todayRevenue = _context.Orders
                .Where(o => o.OrderDate.Date == today)
                .Sum(o => o.TotalPrice);
            var monthlyRevenue = _context.Orders
                .Where(o => o.OrderDate >= thisMonth)
                .Sum(o => o.TotalPrice);

            // Product Data
            var totalProducts = _context.Products.Count();
            var activeProducts = _context.Products.Where(p => p.IsActive).Count();
            var lowStockProducts = _context.Inventories.Where(i => i.StockQuantity < (i.MinimumStockLevel ?? 10)).Count();

            // User Data
            var totalUsers = _context.Users.Count();
            var totalReviews = _context.ProductReviews.Count();

            // Category Data
            var categories = await _categoryRepository.GetAllAsync();

            // Recent Orders
            var recentOrders = await _context.Orders
                .Include(o => o.ApplicationUser)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ToListAsync();

            // Top Products by Sales
            var topProducts = (await _context.OrderDetails
                .GroupBy(od => od.ProductId)
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(od => od.Quantity) })
                .OrderByDescending(x => x.Quantity)
                .Take(5)
                .ToListAsync())
                .Cast<dynamic>()
                .ToList();

            // Monthly Revenue Data (Last 12 months)
            var last12Months = DateTime.UtcNow.AddMonths(-11);
            var monthlyRevenueData = await _context.Orders
                .Where(o => o.OrderDate >= last12Months)
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new MonthlyRevenueDataItem
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Revenue = g.Sum(o => o.TotalPrice),
                    Orders = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            var model = new DashboardViewModel
            {
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                TodayOrders = todayOrders,
                MonthlyOrders = monthlyOrders,
                TotalRevenue = totalRevenue,
                TodayRevenue = todayRevenue,
                MonthlyRevenue = monthlyRevenue,
                TotalProducts = totalProducts,
                ActiveProducts = activeProducts,
                LowStockProducts = lowStockProducts,
                TotalUsers = totalUsers,
                TotalReviews = totalReviews,
                TotalCategories = categories.Count(),
                RecentOrders = recentOrders,
                TopProducts = topProducts,
                MonthlyRevenueData = monthlyRevenueData
            };

            return View(model);
        }

        // Sales Report
        public async Task<IActionResult> SalesReport()
        {
            var monthlyData = await _context.Orders
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Revenue = g.Sum(o => o.TotalPrice),
                    Orders = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            return View(monthlyData);
        }

        // Inventory Report
        public async Task<IActionResult> InventoryReport()
        {
            var lowStockInventory = await _context.Inventories
                .Where(i => i.StockQuantity < (i.MinimumStockLevel ?? 10))
                .ToListAsync();

            return View(lowStockInventory);
        }

        // Customer Report
        public async Task<IActionResult> CustomerReport()
        {
            var topCustomers = await _context.Orders
                .GroupBy(o => o.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    OrderCount = g.Count(),
                    TotalSpent = g.Sum(o => o.TotalPrice)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(20)
                .ToListAsync();

            return View(topCustomers);
        }
    }

    public class MonthlyRevenueDataItem
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }

    public class DashboardViewModel
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int TodayOrders { get; set; }
        public int MonthlyOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int TotalUsers { get; set; }
        public int TotalReviews { get; set; }
        public int TotalCategories { get; set; }
        public List<Order> RecentOrders { get; set; }
        public List<dynamic> TopProducts { get; set; } = new List<dynamic>();
        public List<MonthlyRevenueDataItem> MonthlyRevenueData { get; set; } = new List<MonthlyRevenueDataItem>();
    }
}
