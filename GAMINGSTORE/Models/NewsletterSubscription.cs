using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GAMINGSTORE.Models
{
    public class NewsletterSubscription
    {
        public int Id { get; set; }

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime SubscribedDate { get; set; }

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}
