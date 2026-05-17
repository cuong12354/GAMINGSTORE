using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GAMINGSTORE.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(50)]
        public string? Size { get; set; }

        [StringLength(100)]
        public string? Specification { get; set; }

        public int StockQuantity { get; set; }

        public decimal? PriceModifier { get; set; } // Additional price on top of product price

        public string? SKU { get; set; }

        // Foreign Key
        [ForeignKey("ProductId")]
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public Product? Product { get; set; }
    }
}
