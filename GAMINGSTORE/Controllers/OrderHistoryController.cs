using GAMINGSTORE.Authorization;
using GAMINGSTORE.Data;
using GAMINGSTORE.Extensions;
using GAMINGSTORE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        /// <summary>
        /// Hiển thị danh sách lịch sử đơn hàng
        /// Admin: Xem tất cả đơn hàng | User: Chỉ xem đơn hàng của mình
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // ✅ Logic rõ ràng: Admin xem tất cả, User xem của mình
            IQueryable<Order> query = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.ReturnRequests); // Added to include return requests

            if (!User.HasPermission(PermissionConstants.OrderView))
            {
                query = query.Where(o => o.UserId == user.Id);
            }

            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();

            ViewBag.UserFullName = user.FullName;
            ViewBag.UserEmail = user.Email;
            ViewBag.CanViewAllOrders = User.HasPermission(PermissionConstants.OrderView);

            return View(orders);
        }

        /// <summary>
        /// Hiển thị chi tiết của 1 đơn hàng cụ thể
        /// Admin có thể xem bất kỳ đơn hàng nào, user thường chỉ xem đơn hàng của mình
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var order = await _context.Orders
                .Where(o => o.Id == id && (User.HasPermission(PermissionConstants.OrderView) || o.UserId == user.Id))
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.ReturnRequests)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        /// <summary>
        /// Lấy trạng thái tiến độ của đơn hàng (để cập nhật UI)
        /// Admin: Xem bất kỳ đơn nào | User: Chỉ xem đơn hàng của mình
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetOrderStatus(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            
            // ✅ Logic rõ ràng: Admin xem tất cả, User xem của mình
            var order = await _context.Orders
                .Where(o => o.Id == id && (User.HasPermission(PermissionConstants.OrderView) || o.UserId == user.Id))
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            var statusInfo = new
            {
                orderId = order.Id,
                status = order.Status,
                statusDisplay = GetStatusDisplay(order.Status),
                statusColor = GetStatusColor(order.Status),
                orderDate = order.OrderDate.ToString("dd/MM/yyyy HH:mm"),
                totalPrice = order.TotalPrice,
                customerName = order.CustomerName,
                shippingAddress = order.ShippingAddress,
                paymentMethod = order.PaymentMethod,
                progressPercentage = GetProgressPercentage(order.Status)
            };

            return Ok(statusInfo);
        }

        /// <summary>
        /// Cập nhật trạng thái đơn hàng (chỉ Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = PermissionConstants.OrderManage)]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var validStatuses = new[] { "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled" };
            
            if (!validStatuses.Contains(status))
            {
                return BadRequest("Trạng thái không hợp lệ");
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, newStatus = status, message = $"Cập nhật trạng thái thành: {GetStatusDisplay(status)}" });
        }

        /// <summary>
        /// Hiển thị lịch sử tất cả đơn hàng (chỉ Admin)
        /// </summary>
        [HttpGet]
        [Authorize(Policy = PermissionConstants.OrderView)]
        public async Task<IActionResult> AllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.ApplicationUser)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // ===== HELPER METHODS =====

        private string GetStatusDisplay(string status)
        {
            return status switch
            {
                "Pending" => "Chờ xử lý",
                "Confirmed" => "Đã xác nhận",
                "Shipped" => "Đang giao",
                "Delivered" => "Đã giao",
                "Cancelled" => "Đã hủy",
                _ => "Không xác định"
            };
        }

        private string GetStatusColor(string status)
        {
            return status switch
            {
                "Pending" => "#fbbf24",      // Gold
                "Confirmed" => "#3b82f6",    // Blue
                "Shipped" => "#8b5cf6",      // Purple
                "Delivered" => "#22c55e",    // Green
                "Cancelled" => "#dc2626",    // Red
                _ => "#6b7280"               // Gray
            };
        }

        private int GetProgressPercentage(string status)
        {
            return status switch
            {
                "Pending" => 20,
                "Confirmed" => 40,
                "Shipped" => 70,
                "Delivered" => 100,
                "Cancelled" => 0,
                _ => 0
            };
        }
    }
}
