using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

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
        [Required]
        public string ShippingAddress { get; set; }
        public string? Notes { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public string PaymentMethod { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public string? Status { get; set; }
    }
}