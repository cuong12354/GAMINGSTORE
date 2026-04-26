namespace GAMINGSTORE.Models
{
    public class MemberTier
    {
        public int Id { get; set; }
        public string Name { get; set; } // "Đồng", "Bạc", "Vàng", "Bạch Kim"
        public int MinPoints { get; set; }
        public int MaxPoints { get; set; }
        public decimal DiscountPercentage { get; set; } // 0%, 5%, 10%, 15%
        public string Color { get; set; } // "bronze", "silver", "gold", "platinum"
        public string Description { get; set; }
    }
}
