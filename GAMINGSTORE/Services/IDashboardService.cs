namespace GAMINGSTORE.Services
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        Task<List<RecentOrderDto>> GetRecentOrdersAsync(int count = 5);
        Task<List<PendingReturnDto>> GetPendingReturnsAsync(int count = 5);
        Task<decimal> GetTotalRevenueAsync(int days = 30);
        Task<int> GetTotalProductsAsync();
        Task<int> GetTotalUsersAsync();
        Task<int> GetTotalOrdersAsync();
    }

    public class DashboardStatsDto
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int TotalProducts { get; set; }
        public int TotalUsers { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public int PendingReturns { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public class RecentOrderDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
    }

    public class PendingReturnDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string Reason { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; }
    }
}
