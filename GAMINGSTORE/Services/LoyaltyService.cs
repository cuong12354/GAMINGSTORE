using GAMINGSTORE.Data;
using GAMINGSTORE.Models;
using Microsoft.EntityFrameworkCore;

namespace GAMINGSTORE.Services
{
    public interface ILoyaltyService
    {
        Task AddPointsAsync(string userId, int points, string description, int? orderId = null);
        Task RedeemPointsAsync(string userId, int points, string description);
        Task UpdateMemberTierAsync(string userId);
        Task<LoyaltyPoint[]> GetUserPointsAsync(string userId);
        Task<int> GetUserCurrentPointsAsync(string userId);
        Task<MemberTier> GetUserMemberTierAsync(string userId);
        Task<List<MemberTier>> GetAllMemberTiersAsync();
    }

    public class LoyaltyService : ILoyaltyService
    {
        private readonly ApplicationDbContext _context;

        public LoyaltyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddPointsAsync(string userId, int points, string description, int? orderId = null)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            user.CurrentPoints += points;
            user.TotalPointsEarned += points;

            var loyaltyPoint = new LoyaltyPoint
            {
                UserId = userId,
                Points = points,
                TransactionType = "Purchase",
                Description = description,
                OrderId = orderId,
                CreatedDate = DateTime.Now,
                ExpiryDate = DateTime.Now.AddYears(1) // Điểm hết hạn sau 1 năm
            };

            _context.LoyaltyPoints.Add(loyaltyPoint);
            await _context.SaveChangesAsync();

            // Cập nhật tier nếu cần
            await UpdateMemberTierAsync(userId);
        }

        public async Task RedeemPointsAsync(string userId, int points, string description)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.CurrentPoints < points) return;

            user.CurrentPoints -= points;
            user.TotalPointsRedeemed += points;

            var loyaltyPoint = new LoyaltyPoint
            {
                UserId = userId,
                Points = -points,
                TransactionType = "Redemption",
                Description = description,
                CreatedDate = DateTime.Now
            };

            _context.LoyaltyPoints.Add(loyaltyPoint);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateMemberTierAsync(string userId)
        {
            var user = await _context.Users
                .Include(u => u.MemberTier)
                .FirstOrDefaultAsync(u => u.Id == userId);
            
            if (user == null) return;

            var tiers = await _context.MemberTiers
                .OrderBy(t => t.MinPoints)
                .ToListAsync();

            var newTier = tiers.LastOrDefault(t => user.CurrentPoints >= t.MinPoints) 
                ?? tiers.First();

            if (user.MemberTierId != newTier.Id)
            {
                user.MemberTierId = newTier.Id;
                user.TierUpgradeDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<LoyaltyPoint[]> GetUserPointsAsync(string userId)
        {
            return await _context.LoyaltyPoints
                .Where(lp => lp.UserId == userId)
                .OrderByDescending(lp => lp.CreatedDate)
                .ToArrayAsync();
        }

        public async Task<int> GetUserCurrentPointsAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.CurrentPoints ?? 0;
        }

        public async Task<MemberTier> GetUserMemberTierAsync(string userId)
        {
            var user = await _context.Users
                .Include(u => u.MemberTier)
                .FirstOrDefaultAsync(u => u.Id == userId);
            
            return user?.MemberTier ?? await _context.MemberTiers.FirstOrDefaultAsync();
        }

        public async Task<List<MemberTier>> GetAllMemberTiersAsync()
        {
            return await _context.MemberTiers
                .OrderBy(t => t.MinPoints)
                .ToListAsync();
        }
    }
}
