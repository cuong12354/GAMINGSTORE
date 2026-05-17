using GAMINGSTORE.Authorization;
using GAMINGSTORE.Models;
using GAMINGSTORE.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GAMINGSTORE.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = PermissionConstants.CouponManage)]
    public class CouponController : Controller
    {
        private readonly ICouponRepository _couponRepository;

        public CouponController(ICouponRepository couponRepository)
        {
            _couponRepository = couponRepository;
        }

        // GET: Admin/Coupon
        public async Task<IActionResult> Index(string? status)
        {
            var coupons = await _couponRepository.GetAllAsync();
            var now = DateTime.UtcNow;

            coupons = status switch
            {
                "active" => coupons.Where(c => c.IsActive && c.StartDate <= now && c.ExpiryDate > now).ToList(),
                "inactive" => coupons.Where(c => !c.IsActive).ToList(),
                "expired" => coupons.Where(c => c.ExpiryDate <= now).ToList(),
                "upcoming" => coupons.Where(c => c.StartDate > now).ToList(),
                _ => coupons
            };

            ViewBag.Status = status ?? "all";
            return View(coupons);
        }

        // GET: Admin/Coupon/Create
        public IActionResult Create()
        {
            var coupon = new Coupon
            {
                Code = string.Empty,
                DiscountPercent = 0,
                DiscountAmount = 0,
                MinimumOrderValue = 0,
                MaxUsageCount = 100,
                CurrentUsageCount = 0,
                IsActive = true,
                StartDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(1),
                ApplicableCategoryIds = string.Empty,
                ApplicableProductIds = string.Empty
            };

            return View(coupon);
        }

        // POST: Admin/Coupon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            NormalizeCoupon(coupon);
            ValidateCoupon(coupon);

            var existedCoupon = await _couponRepository.GetByCodeAsync(coupon.Code);
            if (existedCoupon != null)
            {
                ModelState.AddModelError(nameof(Coupon.Code), "Mã coupon này đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                return View(coupon);
            }

            await _couponRepository.AddAsync(coupon);
            TempData["Success"] = $"Đã tạo mã coupon {coupon.Code}.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Coupon/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null)
            {
                return NotFound();
            }

            return View(coupon);
        }

        // POST: Admin/Coupon/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Coupon coupon)
        {
            if (id != coupon.Id)
            {
                return NotFound();
            }

            NormalizeCoupon(coupon);
            ValidateCoupon(coupon);

            var existedCoupon = await _couponRepository.GetByCodeAsync(coupon.Code);
            if (existedCoupon != null && existedCoupon.Id != coupon.Id)
            {
                ModelState.AddModelError(nameof(Coupon.Code), "Mã coupon này đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                return View(coupon);
            }

            await _couponRepository.UpdateAsync(coupon);
            TempData["Success"] = $"Đã cập nhật mã coupon {coupon.Code}.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Coupon/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _couponRepository.DeleteAsync(id);
            TempData["Success"] = "Đã xóa coupon.";
            return RedirectToAction(nameof(Index));
        }

        private static void NormalizeCoupon(Coupon coupon)
        {
            coupon.Code = coupon.Code?.Trim().ToUpper() ?? string.Empty;
            coupon.Description = coupon.Description?.Trim();
            coupon.ApplicableCategoryIds = coupon.ApplicableCategoryIds?.Trim() ?? string.Empty;
            coupon.ApplicableProductIds = coupon.ApplicableProductIds?.Trim() ?? string.Empty;

            if (coupon.DiscountPercent > 0)
            {
                coupon.DiscountAmount = 0;
            }
            else if (coupon.DiscountAmount > 0)
            {
                coupon.DiscountPercent = 0;
            }
        }

        private void ValidateCoupon(Coupon coupon)
        {
            if (string.IsNullOrWhiteSpace(coupon.Code))
            {
                ModelState.AddModelError(nameof(Coupon.Code), "Vui lòng nhập mã coupon.");
            }

            if (coupon.DiscountPercent <= 0 && coupon.DiscountAmount <= 0)
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập giảm theo phần trăm hoặc giảm theo số tiền.");
            }

            if (coupon.DiscountPercent > 0 && coupon.DiscountAmount > 0)
            {
                ModelState.AddModelError(string.Empty, "Chỉ được chọn một loại giảm giá: phần trăm hoặc số tiền.");
            }

            if (coupon.DiscountPercent > 100)
            {
                ModelState.AddModelError(nameof(Coupon.DiscountPercent), "Phần trăm giảm không được vượt quá 100%.");
            }

            if (coupon.MaxUsageCount <= 0)
            {
                ModelState.AddModelError(nameof(Coupon.MaxUsageCount), "Số lượt sử dụng tối đa phải lớn hơn 0.");
            }

            if (coupon.ExpiryDate <= coupon.StartDate)
            {
                ModelState.AddModelError(nameof(Coupon.ExpiryDate), "Ngày hết hạn phải lớn hơn ngày bắt đầu.");
            }
        }
    }
}
