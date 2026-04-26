using GAMINGSTORE.Models;

namespace GAMINGSTORE.Services
{
    public interface INotificationService
    {
        /// <summary>
        /// Gửi thông báo tới người dùng qua Email
        /// </summary>
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody, string userId);

        /// <summary>
        /// Tạo thông báo trong hệ thống (In-App)
        /// </summary>
        Task<CustomerNotification> CreateInAppNotificationAsync(string userId, string title, string message, 
            string notificationType, string? relatedEntityId = null);

        /// <summary>
        /// Lấy danh sách thông báo của người dùng
        /// </summary>
        Task<List<CustomerNotification>> GetUserNotificationsAsync(string userId, int pageSize = 20);

        /// <summary>
        /// Lấy thông báo chưa đọc
        /// </summary>
        Task<List<CustomerNotification>> GetUnreadNotificationsAsync(string userId);

        /// <summary>
        /// Đánh dấu thông báo là đã đọc
        /// </summary>
        Task<bool> MarkAsReadAsync(int notificationId);

        /// <summary>
        /// Lấy template thông báo
        /// </summary>
        Task<NotificationTemplate> GetTemplateAsync(string templateName);

        /// <summary>
        /// Gửi thông báo Return Status (Phê duyệt/Từ chối)
        /// </summary>
        Task SendReturnStatusNotificationAsync(ReturnRequest returnRequest, ApplicationUser user);

        /// <summary>
        /// Lấy số lượng thông báo chưa đọc
        /// </summary>
        Task<int> GetUnreadCountAsync(string userId);
    }
}
