using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GAMINGSTORE.Models
{
    public class ReturnRequest
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required, StringLength(50)]
        public string Status { get; set; } // Pending, Approved, Rejected, Completed

        [StringLength(1000)]
        public string? Reason { get; set; }

        public decimal ReturnAmount { get; set; }

        public DateTime RequestDate { get; set; }

        [StringLength(500)]
        public string? AdminNotes { get; set; }

        // Foreign Keys
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}
