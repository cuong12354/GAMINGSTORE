using GAMINGSTORE.Models;
using Microsoft.EntityFrameworkCore;

namespace GAMINGSTORE.Data
{
public class ApplicationDbContext : DbContext
{
    public
    ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :
    base(options)
    {
    }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
}
}