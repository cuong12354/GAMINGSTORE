using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GAMINGSTORE.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        [Required]
        public string CustomerName { get; set; }
        public string? Phone { get; set; }
        [Required]
        public string ShippingAddress { get; set; }
        public string? Notes { get; set; }
        [ValidateNever]
        public string PaymentMethod { get; set; }
        [ForeignKey("UserId")]
        [JsonIgnore]
        public ApplicationUser ApplicationUser { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public string? Status { get; set; }

        // New Features
        public string? CouponCode { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public decimal MemberDiscountAmount { get; set; } = 0;
        public decimal MemberDiscountPercentage { get; set; } = 0;
        public List<OrderTracking>? TrackingHistory { get; set; }
        public List<ReturnRequest> ReturnRequests { get; set; } = new List<ReturnRequest>();
        public DateTime? DeliveredDate { get; set; }
        public int? EstimatedDaysToDeliver { get; set; }
    }
}