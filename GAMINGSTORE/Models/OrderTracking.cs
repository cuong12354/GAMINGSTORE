using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GAMINGSTORE.Models
{
    public class OrderTracking
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required, StringLength(50)]
        public string Status { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime UpdatedDate { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        // Foreign Key
        [ForeignKey("OrderId")]
        [JsonIgnore]
        public Order? Order { get; set; }
    }
}
