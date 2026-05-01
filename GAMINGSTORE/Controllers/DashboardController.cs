using GAMINGSTORE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GAMINGSTORE.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var stats = await _dashboardService.GetDashboardStatsAsync();
            var recentOrders = await _dashboardService.GetRecentOrdersAsync(5);
            var pendingReturns = await _dashboardService.GetPendingReturnsAsync(5);

            ViewBag.Stats = stats;
            ViewBag.RecentOrders = recentOrders;
            ViewBag.PendingReturns = pendingReturns;

            return View();
        }
    }
}
