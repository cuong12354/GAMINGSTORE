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
        /// Lịch sử đơn hàng cá nhân.
        /// Trang này LUÔN chỉ hiển thị đơn của tài khoản đang đăng nhập.
        /// Admin/Staff muốn xem toàn bộ đơn thì dùng AllOrders hoặc Admin/OrderManagement.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var orders = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.ReturnRequests)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.UserFullName = user.FullName;
            ViewBag.UserEmail = user.Email;
            ViewBag.CanViewAllOrders = false;

            return View(orders);
        }

        /// <summary>
        /// Chi tiết đơn hàng cá nhân.
        /// User chỉ được xem chi tiết đơn hàng của chính mình.
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
                .Where(o => o.Id == id && o.UserId == user.Id)
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
        /// Lấy trạng thái đơn hàng cá nhân cho UI.
        /// User chỉ được lấy trạng thái đơn hàng của chính mình.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetOrderStatus(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var order = await _context.Orders
                .Where(o => o.Id == id && o.UserId == user.Id)
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
        /// Cập nhật trạng thái đơn hàng.
        /// Chỉ tài khoản có quyền OrderManage mới được cập nhật.
        /// Dùng cho AllOrders.cshtml đang gọi bằng fetch.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = PermissionConstants.OrderManage)]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var validStatuses = new[] { "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled" };

            if (string.IsNullOrWhiteSpace(status) || !validStatuses.Contains(status))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Trạng thái không hợp lệ"
                });
            }

            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy đơn hàng"
                });
            }

            order.Status = status;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                newStatus = status,
                statusDisplay = GetStatusDisplay(status),
                message = $"Cập nhật trạng thái thành: {GetStatusDisplay(status)}"
            });
        }

        /// <summary>
        /// Xem tất cả đơn hàng.
        /// Chỉ tài khoản có quyền OrderView mới được xem toàn bộ đơn.
        /// Đây là trang quản trị, không phải lịch sử cá nhân.
        /// </summary>
        [HttpGet]
        [Authorize(Policy = PermissionConstants.OrderView)]
        public async Task<IActionResult> AllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.ApplicationUser)
                .Include(o => o.ReturnRequests)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        private string GetStatusDisplay(string? status)
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

        private string GetStatusColor(string? status)
        {
            return status switch
            {
                "Pending" => "#fbbf24",
                "Confirmed" => "#3b82f6",
                "Shipped" => "#8b5cf6",
                "Delivered" => "#22c55e",
                "Cancelled" => "#dc2626",
                _ => "#6b7280"
            };
        }

        private int GetProgressPercentage(string? status)
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
