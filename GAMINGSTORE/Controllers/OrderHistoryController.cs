using GAMINGSTORE.Data;
using GAMINGSTORE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace GAMINGSTORE.Controllers
{
    [Authorize]
    public class OrderHistoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderHistoryController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: OrderHistory
        public async Task<IActionResult> Index(string? status = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var orders = _context.Orders
                .Where(o => o.UserId == user.Id)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.Status == status);
            }

            var orderList = await orders.ToListAsync();
            ViewData["CurrentStatus"] = status;
            ViewData["StatusFilter"] = new[] { "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled" };

            return View(orderList);
        }

        // GET: OrderHistory/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == user.Id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // GET: OrderHistory/AllOrders (Admin only)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllOrders(string? status = null)
        {
            var orders = _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.ApplicationUser)
                .OrderByDescending(o => o.OrderDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.Status == status);
            }

            var stats = new
            {
                TotalOrders = await _context.Orders.CountAsync(),
                PendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending"),
                CompletedOrders = await _context.Orders.CountAsync(o => o.Status == "Delivered"),
                TotalRevenue = await _context.Orders
                    .Where(o => o.Status == "Delivered")
                    .SumAsync(o => o.TotalPrice)
            };

            ViewData["Stats"] = stats;
            ViewData["CurrentStatus"] = status;
            ViewData["StatusFilter"] = new[] { "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled" };

            return View(await orders.ToListAsync());
        }

        private string GetStatusDisplay(string status)
        {
            return status switch
            {
                "Pending" => "Chờ xác nhận",
                "Confirmed" => "Đã xác nhận",
                "Shipped" => "Đang giao",
                "Delivered" => "Đã giao",
                "Cancelled" => "Đã hủy",
                _ => status
            };
        }
    }
}
