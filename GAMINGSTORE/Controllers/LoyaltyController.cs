using GAMINGSTORE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GAMINGSTORE.Controllers
{
    [Authorize]
    public class LoyaltyController : Controller
    {
        private readonly ILoyaltyService _loyaltyService;

        public LoyaltyController(ILoyaltyService loyaltyService)
        {
            _loyaltyService = loyaltyService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var points = await _loyaltyService.GetUserCurrentPointsAsync(userId);
            var tier = await _loyaltyService.GetUserMemberTierAsync(userId);
            var pointsHistory = await _loyaltyService.GetUserPointsAsync(userId);

            ViewBag.CurrentPoints = points;
            ViewBag.MemberTier = tier;
            ViewBag.PointsHistory = pointsHistory;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RedeemPoints(int points)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "Vui lòng đăng nhập" });

            var currentPoints = await _loyaltyService.GetUserCurrentPointsAsync(userId);
            if (currentPoints < points)
                return Json(new { success = false, message = "Bạn không có đủ điểm" });

            await _loyaltyService.RedeemPointsAsync(userId, points, "Khách hàng yêu cầu đổi điểm");
            return Json(new { success = true, message = "Đã đổi điểm thành công" });
        }

        public async Task<IActionResult> Tiers()
        {
            var tiers = await _loyaltyService.GetAllMemberTiersAsync();
            return View(tiers);
        }
    }
}
