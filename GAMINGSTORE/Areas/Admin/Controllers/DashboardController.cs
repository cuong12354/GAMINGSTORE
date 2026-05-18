using GAMINGSTORE.Authorization;
using GAMINGSTORE.Data;
using GAMINGSTORE.Models;
using GAMINGSTORE.Repositories;
using Microsoft.AspNetCore.Authorization;
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

        public DashboardController(
            ApplicationDbContext context,
            IProductRepository productRepository,
            ICategoryRepository categoryRepository)
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
            var now = DateTime.UtcNow;

            var totalOrders = await _context.Orders.CountAsync();
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending" || o.Status == "PendingReview");
            var confirmedOrders = await _context.Orders.CountAsync(o => o.Status == "Confirmed");
            var todayOrders = await _context.Orders.CountAsync(o => o.OrderDate.Date == today);
            var monthlyOrders = await _context.Orders.CountAsync(o => o.OrderDate >= thisMonth);

            var totalRevenue = await _context.Orders.SumAsync(o => (decimal?)o.TotalPrice) ?? 0;
            var todayRevenue = await _context.Orders
                .Where(o => o.OrderDate.Date == today)
                .SumAsync(o => (decimal?)o.TotalPrice) ?? 0;
            var monthlyRevenue = await _context.Orders
                .Where(o => o.OrderDate >= thisMonth)
                .SumAsync(o => (decimal?)o.TotalPrice) ?? 0;

            var totalProducts = await _context.Products.CountAsync();
            var activeProducts = await _context.Products.CountAsync(p => p.IsActive);
            var inactiveProducts = totalProducts - activeProducts;
            var lowStockProducts = await _context.Inventories.CountAsync(i => i.StockQuantity < (i.MinimumStockLevel ?? 10));
            var outOfStockProducts = await _context.Inventories.CountAsync(i => i.StockQuantity <= 0);

            var totalUsers = await _context.Users.CountAsync();
            var totalReviews = await _context.ProductReviews.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();

            var totalCoupons = await _context.Coupons.CountAsync();
            var activeCoupons = await _context.Coupons.CountAsync(c => c.IsActive && c.StartDate <= now && c.ExpiryDate >= now);
            var expiredCoupons = await _context.Coupons.CountAsync(c => c.ExpiryDate < now);
            var upcomingCoupons = await _context.Coupons.CountAsync(c => c.StartDate > now);

            var totalReturns = await _context.ReturnRequests.CountAsync();
            var pendingReturns = await _context.ReturnRequests.CountAsync(r => r.Status == "Pending");

            var totalWishlistItems = await _context.Wishlists.CountAsync();
            var totalMemberTiers = await _context.MemberTiers.CountAsync();
            var totalLoyaltyTransactions = await _context.LoyaltyPoints.CountAsync();

            var unreadNotifications = await _context.CustomerNotifications.CountAsync(n => !n.IsRead);
            var totalNotifications = await _context.CustomerNotifications.CountAsync();
            var totalAuditLogs = await _context.AuditLogs.CountAsync();
            var totalNewsletterSubscriptions = await _context.NewsletterSubscriptions.CountAsync();

            var recentOrders = await _context.Orders
                .Include(o => o.ApplicationUser)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ToListAsync();

            var lowStockItems = await _context.Inventories
                .Include(i => i.Product)
                .Where(i => i.StockQuantity < (i.MinimumStockLevel ?? 10))
                .OrderBy(i => i.StockQuantity)
                .Take(8)
                .ToListAsync();

            var topProducts = (await _context.OrderDetails
                .GroupBy(od => new { od.ProductId, od.ProductName })
                .Select(g => new TopProductDataItem
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName ?? ("Sản phẩm #" + g.Key.ProductId),
                    Quantity = g.Sum(od => od.Quantity),
                    Revenue = g.Sum(od => od.Price * od.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(6)
                .ToListAsync());

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
                ConfirmedOrders = confirmedOrders,
                TodayOrders = todayOrders,
                MonthlyOrders = monthlyOrders,
                TotalRevenue = totalRevenue,
                TodayRevenue = todayRevenue,
                MonthlyRevenue = monthlyRevenue,
                TotalProducts = totalProducts,
                ActiveProducts = activeProducts,
                InactiveProducts = inactiveProducts,
                LowStockProducts = lowStockProducts,
                OutOfStockProducts = outOfStockProducts,
                TotalUsers = totalUsers,
                TotalReviews = totalReviews,
                TotalCategories = totalCategories,
                TotalCoupons = totalCoupons,
                ActiveCoupons = activeCoupons,
                ExpiredCoupons = expiredCoupons,
                UpcomingCoupons = upcomingCoupons,
                TotalReturns = totalReturns,
                PendingReturns = pendingReturns,
                TotalWishlistItems = totalWishlistItems,
                TotalMemberTiers = totalMemberTiers,
                TotalLoyaltyTransactions = totalLoyaltyTransactions,
                UnreadNotifications = unreadNotifications,
                TotalNotifications = totalNotifications,
                TotalAuditLogs = totalAuditLogs,
                TotalNewsletterSubscriptions = totalNewsletterSubscriptions,
                RecentOrders = recentOrders,
                LowStockItems = lowStockItems,
                TopProducts = topProducts,
                MonthlyRevenueData = monthlyRevenueData
            };

            return View(model);
        }

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

        public async Task<IActionResult> InventoryReport()
        {
            var lowStockInventory = await _context.Inventories
                .Include(i => i.Product)
                .Where(i => i.StockQuantity < (i.MinimumStockLevel ?? 10))
                .OrderBy(i => i.StockQuantity)
                .ToListAsync();

            return View(lowStockInventory);
        }

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

    public class TopProductDataItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
    }

    public class DashboardViewModel
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int TodayOrders { get; set; }
        public int MonthlyOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int InactiveProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public int TotalUsers { get; set; }
        public int TotalReviews { get; set; }
        public int TotalCategories { get; set; }
        public int TotalCoupons { get; set; }
        public int ActiveCoupons { get; set; }
        public int ExpiredCoupons { get; set; }
        public int UpcomingCoupons { get; set; }
        public int TotalReturns { get; set; }
        public int PendingReturns { get; set; }
        public int TotalWishlistItems { get; set; }
        public int TotalMemberTiers { get; set; }
        public int TotalLoyaltyTransactions { get; set; }
        public int UnreadNotifications { get; set; }
        public int TotalNotifications { get; set; }
        public int TotalAuditLogs { get; set; }
        public int TotalNewsletterSubscriptions { get; set; }
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<Inventory> LowStockItems { get; set; } = new List<Inventory>();
        public List<TopProductDataItem> TopProducts { get; set; } = new List<TopProductDataItem>();
        public List<MonthlyRevenueDataItem> MonthlyRevenueData { get; set; } = new List<MonthlyRevenueDataItem>();
    }
}
