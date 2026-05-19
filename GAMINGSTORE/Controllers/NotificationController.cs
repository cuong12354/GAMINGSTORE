using GAMINGSTORE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GAMINGSTORE.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Trang hiển thị danh sách thông báo chưa đọc
        /// </summary>
        [HttpGet("/Notification/GetUnreadNotifications")]
        public async Task<IActionResult> GetUnreadNotificationsPage()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);
            return View("UnreadNotifications", notifications);
        }

        /// <summary>
        /// Get unread count for current user
        /// </summary>
        [HttpGet("api/notification/unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { unreadCount = count });
        }

        /// <summary>
        /// Get recent notifications for current user
        /// </summary>
        [HttpGet("api/notification/recent")]
        public async Task<IActionResult> GetRecentNotifications(int count = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var notifications = await _notificationService.GetUserNotificationsAsync(userId, count);
            return Ok(notifications);
        }

        /// <summary>
        /// Get unread notifications for current user (API)
        /// </summary>
        [HttpGet("api/notification/unread")]
        public async Task<IActionResult> GetUnreadNotificationsApi()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);
            return Ok(notifications);
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        [HttpPost("api/notification/mark-as-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var success = await _notificationService.MarkAsReadAsync(id);
            if (!success)
                return NotFound(new { message = "Thông báo không tồn tại" });

            return Ok(new { message = "Thông báo đã được đánh dấu là đã đọc" });
        }

        /// <summary>
        /// Mark all notifications as read for current user
        /// </summary>
        [HttpPost("api/notification/mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var unreadNotifications = await _notificationService.GetUnreadNotificationsAsync(userId);
            foreach (var notification in unreadNotifications)
            {
                await _notificationService.MarkAsReadAsync(notification.Id);
            }

            return Ok(new { message = "Tất cả thông báo đã được đánh dấu là đã đọc" });
        }
    }
}
