using GAMINGSTORE.Authorization;
using GAMINGSTORE.Data;
using GAMINGSTORE.Models;
using GAMINGSTORE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GAMINGSTORE.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = PermissionConstants.RoleManage)]
    public class LoyaltyController : Controller
    {
        private readonly ILoyaltyService _loyaltyService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public LoyaltyController(
            ILoyaltyService loyaltyService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _loyaltyService = loyaltyService;
            _userManager = userManager;
            _context = context;
        }

        /// <summary>
        /// Xem danh sách hạng thành viên và khách hàng
        /// </summary>
        public async Task<IActionResult> Tiers()
        {
            var tiers = await _loyaltyService.GetAllMemberTiersAsync();
            return View(tiers);
        }

        /// <summary>
        /// Xem chi tiết hạng thành viên và danh sách khách hàng trong hạng đó
        /// </summary>
        public async Task<IActionResult> TierDetails(int id)
        {
            var tier = await _context.MemberTiers.FindAsync(id);
            if (tier == null)
                return NotFound();

            // Lấy danh sách khách hàng trong hạng này
            var customers = await _context.Users
                .Include(u => u.LoyaltyPoints)
                .Where(u => u.LoyaltyPoints.Sum(lp => lp.Points) >= tier.MinPoints && 
                            u.LoyaltyPoints.Sum(lp => lp.Points) < tier.MaxPoints)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.PhoneNumber,
                    TotalPoints = u.LoyaltyPoints.Sum(lp => lp.Points),
                    TotalOrders = u.Orders.Count,
                    TotalSpent = u.Orders.Sum(o => o.TotalPrice),
                    CreatedDate = u.CreatedDate
                })
                .OrderByDescending(c => c.TotalPoints)
                .ToListAsync();

            ViewBag.Tier = tier;
            ViewBag.Customers = customers;
            ViewBag.CustomerCount = customers.Count;

            return View();
        }

        /// <summary>
        /// Xem danh sách tất cả khách hàng với hạng thành viên của họ
        /// </summary>
        public async Task<IActionResult> Members()
        {
            var members = await _context.Users
                .Include(u => u.LoyaltyPoints)
                .Include(u => u.Orders)
                .Where(u => u.LoyaltyPoints.Sum(lp => lp.Points) > 0)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.PhoneNumber,
                    LoyaltyPoints = u.LoyaltyPoints.Sum(lp => lp.Points),
                    TotalOrders = u.Orders.Count,
                    TotalSpent = u.Orders.Sum(o => o.TotalPrice),
                    CreatedDate = u.CreatedDate,
                    LastOrderDate = u.Orders.Max(o => o.OrderDate)
                })
                .OrderByDescending(m => m.LoyaltyPoints)
                .ToListAsync();

            return View(members);
        }

        /// <summary>
        /// Xem chi tiết khách hàng và lịch sử điểm thưởng
        /// </summary>
        public async Task<IActionResult> MemberDetails(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var loyaltyPoints = await _context.LoyaltyPoints
                .Where(lp => lp.UserId == id)
                .OrderByDescending(lp => lp.CreatedDate)
                .ToListAsync();

            var tier = await _loyaltyService.GetUserMemberTierAsync(id);
            var orders = await _context.Orders
                .Where(o => o.UserId == id)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ToListAsync();

            ViewBag.User = user;
            ViewBag.LoyaltyPoints = loyaltyPoints;
            ViewBag.CurrentTier = tier;
            ViewBag.Orders = orders;
            ViewBag.TotalPoints = user.LoyaltyPoints;
            ViewBag.TotalOrders = await _context.Orders.CountAsync(o => o.UserId == id);
            ViewBag.TotalSpent = await _context.Orders.Where(o => o.UserId == id).SumAsync(o => o.TotalPrice);

            return View();
        }

        /// <summary>
        /// Điều chỉnh điểm thưởng của khách hàng
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AdjustPoints(string userId, int points, string reason)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Json(new { success = false, message = "Khách hàng không tồn tại" });

            if (string.IsNullOrEmpty(reason))
                return Json(new { success = false, message = "Vui lòng nhập lý do điều chỉnh" });

            try
            {
                // Tạo bản ghi điểm thưởng mới
                var loyaltyPoint = new LoyaltyPoint
                {
                    UserId = userId,
                    Points = points,
                    TransactionType = "Adjustment",
                    Description = reason,
                    CreatedDate = DateTime.Now
                };

                _context.LoyaltyPoints.Add(loyaltyPoint);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã điều chỉnh điểm thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Xem báo cáo hạng thành viên
        /// </summary>
        public async Task<IActionResult> Report()
        {
            var tiers = await _context.MemberTiers.ToListAsync();
            var tierStats = new List<dynamic>();

            foreach (var tier in tiers)
            {
                var customerCount = await _context.Users
                    .Include(u => u.LoyaltyPoints)
                    .CountAsync(u => u.LoyaltyPoints.Sum(lp => lp.Points) >= tier.MinPoints && 
                                     u.LoyaltyPoints.Sum(lp => lp.Points) < tier.MaxPoints);

                var totalRevenue = await _context.Users
                    .Include(u => u.LoyaltyPoints)
                    .Include(u => u.Orders)
                    .Where(u => u.LoyaltyPoints.Sum(lp => lp.Points) >= tier.MinPoints && 
                                u.LoyaltyPoints.Sum(lp => lp.Points) < tier.MaxPoints)
                    .SelectMany(u => u.Orders)
                    .SumAsync(o => o.TotalPrice);

                tierStats.Add(new
                {
                    TierId = tier.Id,
                    TierName = tier.Name,
                    DiscountPercentage = tier.DiscountPercentage,
                    CustomerCount = customerCount,
                    TotalRevenue = totalRevenue,
                    AverageSpent = customerCount > 0 ? totalRevenue / customerCount : 0
                });
            }

            ViewBag.TierStats = tierStats;
            ViewBag.TotalMembers = await _context.Users.Include(u => u.LoyaltyPoints).CountAsync(u => u.LoyaltyPoints.Sum(lp => lp.Points) > 0);
            ViewBag.TotalPoints = await _context.LoyaltyPoints.SumAsync(lp => lp.Points);

            return View();
        }
    }
}
