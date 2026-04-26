using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GAMINGSTORE.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }

        [Required]
        [MaxLength(256)]
        public string ActionType { get; set; } // Create, Update, Delete

        [Required]
        [MaxLength(256)]
        public string EntityName { get; set; } // Product, Coupon, Order, etc.

        [Required]
        public int EntityId { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        // For tracking changes
        public string OldValues { get; set; } // JSON format

        public string NewValues { get; set; } // JSON format

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string IpAddress { get; set; }

        [MaxLength(256)]
        public string UserAgent { get; set; }
    }
}
