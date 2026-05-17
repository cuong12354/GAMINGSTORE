using GAMINGSTORE.Authorization;
using GAMINGSTORE.Extensions;
using GAMINGSTORE.Models;
using GAMINGSTORE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GAMINGSTORE.Controllers
{
    [Authorize]
    public class ReturnController : Controller
    {
        private readonly IReturnService _returnService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        public ReturnController(
            IReturnService returnService,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService
        )
        {
            _returnService = returnService;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Display list of user's return requests
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Identity/Account");

            var returns = await _returnService.GetUserReturnRequestsAsync(userId);
            return View(returns);
        }

        /// <summary>
        /// View details of a specific return request
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var returnRequest = await _returnService.GetReturnRequestByIdAsync(id);
            if (returnRequest == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (returnRequest.UserId != userId && !User.HasPermission(PermissionConstants.ReturnManage))
                return Unauthorized();

            return View(returnRequest);
        }

        /// <summary>
        /// Display form to create new return request
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create(int orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Check if order can be returned
            bool canReturn = await _returnService.CanReturnOrderAsync(orderId);
            if (!canReturn)
            {
                TempData["Error"] = "❌ Đơn hàng không thể trả lại. Vui lòng kiểm tra lại hoặc liên hệ hỗ trợ.";
                return RedirectToAction("Index", "OrderHistory");
            }

            // Calculate refund amount
            decimal refundAmount = await _returnService.CalculateRefundAmountAsync(orderId);

            ViewBag.OrderId = orderId;
            ViewBag.RefundAmount = refundAmount;

            return View();
        }

        /// <summary>
        /// Process return request submission
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int orderId, string reason)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Identity/Account");

            // Calculate refund amount
            decimal refundAmount = await _returnService.CalculateRefundAmountAsync(orderId);

            // Create return request
            var returnRequest = await _returnService.CreateReturnRequestAsync(
                userId,
                orderId,
                reason,
                refundAmount
            );

            if (returnRequest == null)
            {
                TempData["Error"] = "❌ Không thể tạo yêu cầu trả hàng. Vui lòng thử lại.";
                return RedirectToAction("Index");
            }

            TempData["Success"] = "✅ Yêu cầu trả hàng của bạn đã được gửi. Chúng tôi sẽ xem xét trong 3-5 ngày làm việc.";
            return RedirectToAction("Details", new { id = returnRequest.Id });
        }

        /// <summary>
        /// Admin: View all pending returns
        /// </summary>
        [Authorize(Policy = PermissionConstants.ReturnManage)]
        public async Task<IActionResult> Pending()
        {
            var pendingReturns = await _returnService.GetPendingReturnRequestsAsync();
            return View(pendingReturns);
        }

        /// <summary>
        /// Admin: Approve or reject return request
        /// </summary>
        [Authorize(Policy = PermissionConstants.ReturnManage)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, string? adminNotes)
        {
            var success = await _returnService.UpdateReturnStatusAsync(id, status, adminNotes);
            
            if (!success)
            {
                TempData["Error"] = "❌ Không thể cập nhật trạng thái.";
            }
            else
            {
                // Get return request details and send notification
                var returnRequest = await _returnService.GetReturnRequestByIdAsync(id);
                if (returnRequest != null)
                {
                    var user = await _userManager.FindByIdAsync(returnRequest.UserId);
                    if (user != null)
                    {
                        // Send notification to user
                        await _notificationService.SendReturnStatusNotificationAsync(returnRequest, user);
                    }
                }

                TempData["Success"] = "✅ Trạng thái đã được cập nhật và thông báo đã được gửi.";
            }

            return RedirectToAction("Pending");
        }

        /// <summary>
        /// Admin: View return statistics
        /// </summary>
        [Authorize(Policy = PermissionConstants.ReturnManage)]
        public async Task<IActionResult> Statistics()
        {
            var stats = await _returnService.GetReturnStatisticsAsync();
            return View(stats);
        }
    }
}
