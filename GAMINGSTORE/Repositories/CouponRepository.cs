using GAMINGSTORE.Models;
using GAMINGSTORE.Data;

namespace GAMINGSTORE.Repositories
{
    public interface ICouponRepository
    {
        Task<Coupon> GetByCodeAsync(string code);
        Task<List<Coupon>> GetActiveAsync();
        Task<Coupon> GetByIdAsync(int id);
        Task AddAsync(Coupon coupon);
        Task UpdateAsync(Coupon coupon);
        Task DeleteAsync(int id);
        Task<bool> ValidateCouponAsync(string code);
    }

    public class CouponRepository : ICouponRepository
    {
        private readonly ApplicationDbContext _context;

        public CouponRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Coupon> GetByCodeAsync(string code)
        {
            return await Task.FromResult(_context.Coupons
                .FirstOrDefault(c => c.Code.ToUpper() == code.ToUpper()));
        }

        public async Task<List<Coupon>> GetActiveAsync()
        {
            var now = DateTime.UtcNow;
            return await Task.FromResult(_context.Coupons
                .Where(c => c.IsActive && c.StartDate <= now && c.ExpiryDate > now)
                .ToList());
        }

        public async Task<Coupon> GetByIdAsync(int id)
        {
            return await Task.FromResult(_context.Coupons.Find(id));
        }

        public async Task AddAsync(Coupon coupon)
        {
            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Coupon coupon)
        {
            _context.Coupons.Update(coupon);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var coupon = await GetByIdAsync(id);
            if (coupon != null)
            {
                _context.Coupons.Remove(coupon);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ValidateCouponAsync(string code)
        {
            var now = DateTime.UtcNow;
            var coupon = await GetByCodeAsync(code);
            return coupon != null 
                && coupon.IsActive 
                && coupon.StartDate <= now 
                && coupon.ExpiryDate > now
                && coupon.CurrentUsageCount < coupon.MaxUsageCount;
        }
    }
}
