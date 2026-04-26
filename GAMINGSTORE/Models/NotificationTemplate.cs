using System.ComponentModel.DataAnnotations;

namespace GAMINGSTORE.Models
{
    public class NotificationTemplate
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } // e.g., "ReturnApproved", "OrderShipped"

        [Required, StringLength(500)]
        public string Subject { get; set; } // Email subject with placeholders: {OrderId}, {CustomerName}

        [Required]
        public string Body { get; set; } // HTML body with placeholders

        [Required, StringLength(50)]
        public string Type { get; set; } // "Email", "SMS", "InApp"

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ModifiedDate { get; set; }
    }
}
