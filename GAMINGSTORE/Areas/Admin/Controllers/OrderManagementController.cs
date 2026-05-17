using GAMINGSTORE.Authorization;
using GAMINGSTORE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GAMINGSTORE.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = PermissionConstants.OrderManage)]
    public class OrderManagementController : Controller
    {
        private readonly IOrderManagementService _orderManagementService;
        private readonly ILogger<OrderManagementController> _logger;

        public OrderManagementController(IOrderManagementService orderManagementService, ILogger<OrderManagementController> logger)
        {
            _orderManagementService = orderManagementService;
            _logger = logger;
        }

        /// <summary>
        /// List all orders
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var orders = await _orderManagementService.GetAllOrdersAsync();
            return View(orders);
        }

        /// <summary>
        /// View order details and update status
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderManagementService.GetOrderByIdAsync(id);
            if (order == null)
            {
                TempData["Error"] = "❌ Không tìm thấy đơn hàng.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.AvailableStatuses = await _orderManagementService.GetAvailableStatusesAsync();
            return View(order);
        }

        /// <summary>
        /// Update order status (POST)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string newStatus)
        {
            var success = await _orderManagementService.UpdateOrderStatusAsync(id, newStatus);

            if (!success)
            {
                TempData["Error"] = "❌ Không thể cập nhật trạng thái đơn hàng.";
            }
            else
            {
                TempData["Success"] = $"✅ Trạng thái đơn hàng đã cập nhật thành: {newStatus}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
