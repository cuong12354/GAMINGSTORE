using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GAMINGSTORE.Models
{
    public class Coupon
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Code { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercent { get; set; }

        [Range(0, 1000000)]
        public decimal DiscountAmount { get; set; }

        [Range(0, 1000000)]
        public decimal MinimumOrderValue { get; set; }

        public int MaxUsageCount { get; set; }

        public int CurrentUsageCount { get; set; } = 0;

        public DateTime StartDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
