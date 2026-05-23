using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GAMINGSTORE.Models
{
    public class CustomerNotification
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        [Required, StringLength(50)]
        public string Type { get; set; } // "ReturnApproved", "OrderShipped", "LoyaltyPoints", etc.

        [Required, StringLength(50)]
        public string Channel { get; set; } // "Email", "SMS", "InApp"

        public bool IsRead { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ReadDate { get; set; }

        public string? RelatedEntityId { get; set; } // OrderId, ReturnRequestId, etc.

        // Foreign Keys
        [ForeignKey("UserId")]
        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public ApplicationUser User { get; set; }
    }
}
