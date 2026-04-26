using GAMINGSTORE.Models;
using GAMINGSTORE.Data;

namespace GAMINGSTORE.Repositories
{
    public interface IWishlistRepository
    {
        Task<List<Wishlist>> GetUserWishlistAsync(string userId);
        Task<Wishlist> GetByUserAndProductAsync(string userId, int productId);
        Task AddAsync(Wishlist wishlist);
        Task RemoveAsync(int id);
        Task<bool> IsInWishlistAsync(string userId, int productId);
    }

    public class WishlistRepository : IWishlistRepository
    {
        private readonly ApplicationDbContext _context;

        public WishlistRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Wishlist>> GetUserWishlistAsync(string userId)
        {
            return await Task.FromResult(_context.Wishlists
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.AddedDate)
                .ToList());
        }

        public async Task<Wishlist> GetByUserAndProductAsync(string userId, int productId)
        {
            return await Task.FromResult(_context.Wishlists
                .FirstOrDefault(w => w.UserId == userId && w.ProductId == productId));
        }

        public async Task AddAsync(Wishlist wishlist)
        {
            var existing = await GetByUserAndProductAsync(wishlist.UserId, wishlist.ProductId);
            if (existing == null)
            {
                wishlist.AddedDate = DateTime.UtcNow;
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveAsync(int id)
        {
            var wishlist = _context.Wishlists.Find(id);
            if (wishlist != null)
            {
                _context.Wishlists.Remove(wishlist);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsInWishlistAsync(string userId, int productId)
        {
            var item = await GetByUserAndProductAsync(userId, productId);
            return item != null;
        }
    }
}
