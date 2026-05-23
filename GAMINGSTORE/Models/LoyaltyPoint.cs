namespace GAMINGSTORE.Models
{
    using System;
    using System.Text.Json.Serialization;

    public class LoyaltyPoint
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        [JsonIgnore]
        public ApplicationUser User { get; set; }
        public int Points { get; set; }
        public string TransactionType { get; set; } // "Purchase", "Redemption", "Bonus", "Adjustment"
        public int? OrderId { get; set; }
        [JsonIgnore]
        public Order Order { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ExpiryDate { get; set; } // Điểm có thể hết hạn
        public int? MemberTierId { get; set; }
        [JsonIgnore]
        public MemberTier MemberTier { get; set; }
    }
}
