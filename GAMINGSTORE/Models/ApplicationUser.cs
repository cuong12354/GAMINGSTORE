using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GAMINGSTORE.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? Age { get; set; }

        // Profile Enhancement
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? CompanyName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? ProfileImage { get; set; }
        public DateTime CreatedDate { get; set; }

        // Relationships
        [JsonIgnore]
        public List<ProductReview>? Reviews { get; set; }
        [JsonIgnore]
        public List<Wishlist>? WishlistItems { get; set; }
        [JsonIgnore]
        public List<NewsletterSubscription>? NewsletterSubscriptions { get; set; }
        [JsonIgnore]
        public List<ReturnRequest>? ReturnRequests { get; set; }
        [JsonIgnore]
        public List<Order>? Orders { get; set; }
        [JsonIgnore]
        public List<LoyaltyPoint>? LoyaltyPoints { get; set; }
        [JsonIgnore]
        public List<AuditLog>? AuditLogs { get; set; }

        // Statistics
        public int TotalOrders { get; set; } = 0;
        public decimal TotalSpent { get; set; } = 0;
        public int TotalReviews { get; set; } = 0;

        // Loyalty Program
        public int CurrentPoints { get; set; } = 0;
        public int TotalPointsEarned { get; set; } = 0;
        public int TotalPointsRedeemed { get; set; } = 0;
        public int MemberTierId { get; set; } = 1; // Default: Đồng
        public MemberTier? MemberTier { get; set; }
        public DateTime? MemberSinceDate { get; set; }
        public DateTime? TierUpgradeDate { get; set; }
        public bool IsVip { get; set; } = false;
    }
}
