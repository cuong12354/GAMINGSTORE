using GAMINGSTORE.Models;
using System.ComponentModel.DataAnnotations;

namespace GAMINGSTORE.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string? Name { get; set; }

        // ĐÃ SỬA: Chỉ giữ lại 1 dòng này là đủ để Entity Framework hiểu quan hệ Nhiều - Nhiều
        public List<Product> Products { get; set; } = new List<Product>();
    }
}