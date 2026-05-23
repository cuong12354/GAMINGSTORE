using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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

        [JsonIgnore]
        public List<ProductImage>? Images { get; set; }

        // CHỈ GIỮ LẠI ĐÚNG 1 DÒNG NÀY (Để thể hiện 1 Sản phẩm có Nhiều Danh mục)
        [JsonIgnore]
        public List<Category> Categories { get; set; } = new List<Category>();

        // New Features
        public decimal AverageRating { get; set; } = 0;

        public int ReviewCount { get; set; } = 0;

        [JsonIgnore]
        public List<ProductReview>? Reviews { get; set; }

        [JsonIgnore]
        public List<Wishlist>? WishlistItems { get; set; }

        [JsonIgnore]
        public List<ProductVariant>? Variants { get; set; }

        [JsonIgnore]
        public Inventory? Inventory { get; set; }

        [StringLength(200)]
        public string? SKU { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }
    }
}