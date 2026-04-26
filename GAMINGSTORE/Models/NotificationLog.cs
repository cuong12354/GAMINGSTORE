using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GAMINGSTORE.Models
{
    public class NotificationLog
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required, StringLength(100)]
        public string NotificationType { get; set; } // "ReturnApproved", "OrderShipped", etc.

        [Required, StringLength(50)]
        public string Channel { get; set; } // "Email", "SMS", "InApp"

        [Required]
        public string Recipient { get; set; } // Email address or phone number

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }

        [Required, StringLength(20)]
        public string Status { get; set; } // "Sent", "Failed", "Pending"

        public string? ErrorMessage { get; set; }

        public DateTime SentDate { get; set; } = DateTime.Now;

        public int? RetryCount { get; set; } = 0;

        public string? ExternalId { get; set; } // Email service message ID

        // Foreign Keys
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}
