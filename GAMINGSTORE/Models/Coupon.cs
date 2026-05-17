using System.ComponentModel.DataAnnotations;

namespace GAMINGSTORE.Models
{
    public class Coupon
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã coupon.")]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0, 100, ErrorMessage = "Phần trăm giảm phải từ 0 đến 100.")]
        public decimal DiscountPercent { get; set; } = 0;

        [Range(typeof(decimal), "0", "1000000000", ErrorMessage = "Số tiền giảm phải từ 0 đến 1 tỷ.")]
        public decimal DiscountAmount { get; set; } = 0;

        [Range(typeof(decimal), "0", "1000000000", ErrorMessage = "Giá trị đơn tối thiểu phải từ 0 đến 1 tỷ.")]
        public decimal MinimumOrderValue { get; set; } = 0;

        public int MaxUsageCount { get; set; } = 999;

        public int CurrentUsageCount { get; set; } = 0;

        public DateTime StartDate { get; set; } = DateTime.Now;

        public DateTime ExpiryDate { get; set; } = DateTime.Now.AddMonths(1);

        public bool IsActive { get; set; } = true;

        [StringLength(1000)]
        public string? ApplicableProductIds { get; set; } = "";

        [StringLength(500)]
        public string? ApplicableCategoryIds { get; set; } = "";
    }
}