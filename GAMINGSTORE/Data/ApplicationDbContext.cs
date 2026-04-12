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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<CartItem>();
            
            // Configure Many-to-Many relationship between Product and Category
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Categories)
                .WithMany(c => c.Products)
                .UsingEntity(j => j.ToTable("CategoryProduct"));
            
            // Configure IdentityUser properties to allow NULL for OAuth users
            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.PasswordHash)
                .IsRequired(false);
            
            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.SecurityStamp)
                .IsRequired(false);
            
            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.ConcurrencyStamp)
                .IsRequired(false);
            
            base.OnModelCreating(modelBuilder);
        }
    }
}