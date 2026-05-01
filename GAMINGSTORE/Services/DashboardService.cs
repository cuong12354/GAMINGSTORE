using GAMINGSTORE.Data;
using GAMINGSTORE.Models;
using Microsoft.EntityFrameworkCore;

namespace GAMINGSTORE.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var totalOrders = await _context.Orders.CountAsync();
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending");
            var totalProducts = await _context.Products.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalRevenue = await _context.Orders.SumAsync(o => (decimal?)o.TotalPrice) ?? 0;
            var thisMonth = DateTime.Now.AddMonths(-1);
            var revenueThisMonth = await _context.Orders
                .Where(o => o.OrderDate >= thisMonth)
                .SumAsync(o => (decimal?)o.TotalPrice) ?? 0;
            var pendingReturns = await _context.ReturnRequests.CountAsync(r => r.Status == "Pending");
            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            return new DashboardStatsDto
            {
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                TotalProducts = totalProducts,
                TotalUsers = totalUsers,
                TotalRevenue = totalRevenue,
                RevenueThisMonth = revenueThisMonth,
                PendingReturns = pendingReturns,
                AverageOrderValue = averageOrderValue
            };
        }

        public async Task<List<RecentOrderDto>> GetRecentOrdersAsync(int count = 5)
        {
            var orders = await _context.Orders
                .Include(o => o.ApplicationUser)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .Select(o => new RecentOrderDto
                {
                    Id = o.Id,
                    CustomerName = o.ApplicationUser != null ? (o.ApplicationUser.FullName ?? o.ApplicationUser.UserName) : o.CustomerName,
                    OrderDate = o.OrderDate,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status ?? "Pending"
                })
                .ToListAsync();

            return orders;
        }

        public async Task<List<PendingReturnDto>> GetPendingReturnsAsync(int count = 5)
        {
            var returns = await _context.ReturnRequests
                .Include(r => r.User)
                .Include(r => r.Order)
                .Where(r => r.Status == "Pending")
                .OrderByDescending(r => r.RequestDate)
                .Take(count)
                .Select(r => new PendingReturnDto
                {
                    Id = r.Id,
                    OrderId = r.OrderId,
                    CustomerName = r.User.FullName ?? r.User.UserName,
                    Reason = r.Reason,
                    RequestDate = r.RequestDate,
                    Status = r.Status
                })
                .ToListAsync();

            return returns;
        }

        public async Task<decimal> GetTotalRevenueAsync(int days = 30)
        {
            var startDate = DateTime.Now.AddDays(-days);
            var revenue = await _context.Orders
                .Where(o => o.OrderDate >= startDate)
                .SumAsync(o => (decimal?)o.TotalPrice) ?? 0;

            return revenue;
        }

        public async Task<int> GetTotalProductsAsync()
        {
            return await _context.Products.CountAsync();
        }

        public async Task<int> GetTotalUsersAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<int> GetTotalOrdersAsync()
        {
            return await _context.Orders.CountAsync();
        }
    }
}
