using GAMINGSTORE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace GAMINGSTORE.Data
{
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
    public
    ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :
    base(options)
    {
    }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<ProductReview> ProductReviews { get; set; }
    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<OrderTracking> OrderTrackings { get; set; }
    public DbSet<ReturnRequest> ReturnRequests { get; set; }
    public DbSet<NewsletterSubscription> NewsletterSubscriptions { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<MemberTier> MemberTiers { get; set; }
    public DbSet<LoyaltyPoint> LoyaltyPoints { get; set; }
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
    public DbSet<CustomerNotification> CustomerNotifications { get; set; }
    public DbSet<NotificationLog> NotificationLogs { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<CartItem>();
            
            // Configure Many-to-Many relationship between Product and Category
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Categories)
                .WithMany(c => c.Products)
                .UsingEntity(j => j.ToTable("CategoryProduct"));

            // Configure Self-Referencing relationship for Category
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Parent)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure One-to-Many relationships
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Reviews)
                .WithOne(r => r.Product)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.WishlistItems)
                .WithOne(w => w.Product)
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.Variants)
                .WithOne(v => v.Product)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Inventory)
                .WithOne(i => i.Product)
                .HasForeignKey<Inventory>(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Reviews)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.WishlistItems)
                .WithOne(w => w.User)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.TrackingHistory)
                .WithOne(t => t.Order)
                .HasForeignKey(t => t.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.ReturnRequests)
                .WithOne(r => r.Order)
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.AuditLogs)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✨ Configure decimal precision
            modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);
            modelBuilder.Entity<Order>().Property(o => o.TotalPrice).HasPrecision(18, 2);
            modelBuilder.Entity<Order>().Property(o => o.DiscountAmount).HasPrecision(18, 2);
            modelBuilder.Entity<OrderDetail>().Property(od => od.Price).HasPrecision(18, 2);
            modelBuilder.Entity<Coupon>().Property(c => c.DiscountAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Coupon>().Property(c => c.MinimumOrderValue).HasPrecision(18, 2);
            modelBuilder.Entity<ReturnRequest>().Property(r => r.ReturnAmount).HasPrecision(18, 2);
            
            base.OnModelCreating(modelBuilder);
        }
    }
}