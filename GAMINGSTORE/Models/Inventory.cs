using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GAMINGSTORE.Models
{
    public class Inventory
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int StockQuantity { get; set; }

        public int ReservedQuantity { get; set; } = 0;

        public int? MinimumStockLevel { get; set; }

        public DateTime LastRestockDate { get; set; }

        public DateTime LastSoldDate { get; set; }

        [StringLength(500)]
        public string? WarehouseLocation { get; set; }

        // Foreign Key
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }
}
