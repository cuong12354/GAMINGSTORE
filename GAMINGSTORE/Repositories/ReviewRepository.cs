using GAMINGSTORE.Models;
using GAMINGSTORE.Data;

namespace GAMINGSTORE.Repositories
{
    public interface IReviewRepository
    {
        Task<IEnumerable<ProductReview>> GetByProductIdAsync(int productId);
        Task<List<ProductReview>> GetUserReviewsAsync(string userId);
        Task<ProductReview> GetByIdAsync(int id);
        Task AddAsync(ProductReview review);
        Task UpdateAsync(ProductReview review);
        Task DeleteAsync(int id);
        Task<double> GetAverageRatingAsync(int productId);
        Task<int> GetReviewCountAsync(int productId);
    }

    public class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationDbContext _context;

        public ReviewRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductReview>> GetByProductIdAsync(int productId)
        {
            return await Task.FromResult(_context.ProductReviews
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedDate)
                .AsEnumerable());
        }

        public async Task<List<ProductReview>> GetUserReviewsAsync(string userId)
        {
            return await Task.FromResult(_context.ProductReviews
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedDate)
                .ToList());
        }

        public async Task<ProductReview> GetByIdAsync(int id)
        {
            return await Task.FromResult(_context.ProductReviews.Find(id));
        }

        public async Task AddAsync(ProductReview review)
        {
            review.CreatedDate = DateTime.UtcNow;
            _context.ProductReviews.Add(review);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductReview review)
        {
            _context.ProductReviews.Update(review);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var review = await GetByIdAsync(id);
            if (review != null)
            {
                _context.ProductReviews.Remove(review);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<double> GetAverageRatingAsync(int productId)
        {
            var reviews = await GetByProductIdAsync(productId);
            return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
        }

        public async Task<int> GetReviewCountAsync(int productId)
        {
            var reviews = await GetByProductIdAsync(productId);
            return reviews.Count();
        }
    }
}
