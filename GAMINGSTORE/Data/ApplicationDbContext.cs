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
    public DbSet<Order> Order { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<CartItem>();
            base.OnModelCreating(modelBuilder);
        }
    }
}