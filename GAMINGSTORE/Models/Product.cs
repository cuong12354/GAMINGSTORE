using System.ComponentModel.DataAnnotations;

namespace GAMINGSTORE.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string? Name { get; set; }
        [Range(1, 1000000000)]
        public decimal Price { get; set; } 
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public List<ProductImage>? Images { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
