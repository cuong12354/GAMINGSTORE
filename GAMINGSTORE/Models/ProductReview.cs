using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GAMINGSTORE.Models
{
    public class ProductReview
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(500)]
        public string? Title { get; set; }

        [StringLength(2000)]
        public string? Comment { get; set; }

        public DateTime CreatedDate { get; set; }

        public int HelpfulCount { get; set; } = 0;

        public bool IsVerifiedPurchase { get; set; }

        // Foreign Keys
        [ForeignKey("ProductId")]
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public Product? Product { get; set; }

        [ForeignKey("UserId")]
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public ApplicationUser? User { get; set; }
    }
}
