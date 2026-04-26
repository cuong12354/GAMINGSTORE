using GAMINGSTORE.Data;
using GAMINGSTORE.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;

namespace GAMINGSTORE.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ApplicationDbContext context, IConfiguration configuration, ILogger<NotificationService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Gửi email thông báo
        /// </summary>
        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody, string userId)
        {
            try
            {
                var smtpServer = _configuration["Smtp:Server"];
                var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
                var senderEmail = _configuration["Smtp:SenderEmail"];
                var senderPassword = _configuration["Smtp:SenderPassword"];
                var senderName = _configuration["Smtp:SenderName"] ?? "Gaming Store";

                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(senderEmail, senderPassword);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail, senderName),
                        Subject = subject,
                        Body = htmlBody,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                }

                // Log the sent notification
                var log = new NotificationLog
                {
                    UserId = userId,
                    NotificationType = "Email",
                    Channel = "Email",
                    Recipient = toEmail,
                    Subject = subject,
                    Body = htmlBody,
                    Status = "Sent",
                    SentDate = DateTime.Now
                };
                _context.NotificationLogs.Add(log);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Email sent successfully to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email to {toEmail}: {ex.Message}");
                
                // Log failed notification attempt
                var log = new NotificationLog
                {
                    UserId = userId,
                    NotificationType = "Email",
                    Channel = "Email",
                    Recipient = toEmail,
                    Subject = subject,
                    Body = htmlBody,
                    Status = "Failed",
                    ErrorMessage = ex.Message,
                    SentDate = DateTime.Now
                };
                _context.NotificationLogs.Add(log);
                await _context.SaveChangesAsync();

                return false;
            }
        }

        /// <summary>
        /// Tạo thông báo In-App
        /// </summary>
        public async Task<CustomerNotification> CreateInAppNotificationAsync(string userId, string title, string message,
            string notificationType, string? relatedEntityId = null)
        {
            var notification = new CustomerNotification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = notificationType,
                Channel = "InApp",
                IsRead = false,
                CreatedDate = DateTime.Now,
                RelatedEntityId = relatedEntityId
            };

            _context.CustomerNotifications.Add(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"In-app notification created for user {userId}: {title}");
            return notification;
        }

        /// <summary>
        /// Lấy danh sách thông báo của user
        /// </summary>
        public async Task<List<CustomerNotification>> GetUserNotificationsAsync(string userId, int pageSize = 20)
        {
            return await _context.CustomerNotifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedDate)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy thông báo chưa đọc
        /// </summary>
        public async Task<List<CustomerNotification>> GetUnreadNotificationsAsync(string userId)
        {
            return await _context.CustomerNotifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Đánh dấu thông báo là đã đọc
        /// </summary>
        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.CustomerNotifications.FindAsync(notificationId);
            if (notification == null)
                return false;

            notification.IsRead = true;
            notification.ReadDate = DateTime.Now;
            _context.CustomerNotifications.Update(notification);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Lấy template thông báo
        /// </summary>
        public async Task<NotificationTemplate> GetTemplateAsync(string templateName)
        {
            return await _context.NotificationTemplates
                .FirstOrDefaultAsync(t => t.Name == templateName && t.IsActive);
        }

        /// <summary>
        /// Gửi thông báo trạng thái Return
        /// </summary>
        public async Task SendReturnStatusNotificationAsync(ReturnRequest returnRequest, ApplicationUser user)
        {
            try
            {
                string notificationType = returnRequest.Status switch
                {
                    "Approved" => "ReturnApproved",
                    "Rejected" => "ReturnRejected",
                    "Completed" => "ReturnCompleted",
                    _ => "ReturnStatusChanged"
                };

                string title = returnRequest.Status switch
                {
                    "Approved" => "✅ Yêu cầu trả hàng được phê duyệt",
                    "Rejected" => "❌ Yêu cầu trả hàng bị từ chối",
                    "Completed" => "✔️ Trả hàng đã hoàn tất",
                    _ => "📋 Trạng thái yêu cầu trả hàng thay đổi"
                };

                string message = returnRequest.Status switch
                {
                    "Approved" => $"Yêu cầu trả hàng cho đơn hàng #{returnRequest.OrderId} đã được phê duyệt. Số tiền hoàn lại: {returnRequest.ReturnAmount:N0}₫",
                    "Rejected" => $"Yêu cầu trả hàng cho đơn hàng #{returnRequest.OrderId} bị từ chối. Lý do: {returnRequest.AdminNotes}",
                    "Completed" => $"Trả hàng cho đơn hàng #{returnRequest.OrderId} đã hoàn tất.",
                    _ => $"Trạng thái yêu cầu trả hàng #{returnRequest.Id} thay đổi thành: {returnRequest.Status}"
                };

                // Create in-app notification
                await CreateInAppNotificationAsync(
                    returnRequest.UserId,
                    title,
                    message,
                    notificationType,
                    $"ReturnRequest-{returnRequest.Id}"
                );

                // Send email notification
                string emailSubject = $"[Gaming Store] {title}";
                string emailBody = $@"
                <h2>{title}</h2>
                <p>{message}</p>
                <p>Chi tiết yêu cầu trả hàng:</p>
                <ul>
                    <li><strong>Đơn hàng:</strong> #{returnRequest.OrderId}</li>
                    <li><strong>Số tiền:</strong> {returnRequest.ReturnAmount:N0}₫</li>
                    <li><strong>Trạng thái:</strong> {returnRequest.Status}</li>
                    <li><strong>Lý do:</strong> {returnRequest.Reason}</li>
                </ul>
                <p><a href='http://localhost:5190/Return/Details/{returnRequest.Id}'>Xem chi tiết</a></p>";

                await SendEmailAsync(user.Email, emailSubject, emailBody, user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending return notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy số lượng thông báo chưa đọc
        /// </summary>
        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.CustomerNotifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }
    }
}
