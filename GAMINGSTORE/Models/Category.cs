using GAMINGSTORE.Models;
using System.ComponentModel.DataAnnotations;

namespace GAMINGSTORE.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string? Name { get; set; }

        // Hierarchy support
        public int? ParentId { get; set; }
        public Category? Parent { get; set; }
        public List<Category> SubCategories { get; set; } = new List<Category>();

        // Menu UI support
        public string? Icon { get; set; } // Bootstrap icon class or image URL
        public int DisplayOrder { get; set; } = 0;
        public bool IsMenuVisible { get; set; } = true;

        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public List<Product> Products { get; set; } = new List<Product>();
    }
}