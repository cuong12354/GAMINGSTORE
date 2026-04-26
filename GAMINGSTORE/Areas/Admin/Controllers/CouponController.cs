using GAMINGSTORE.Models;
using GAMINGSTORE.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GAMINGSTORE.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CouponController : Controller
    {
        private readonly ICouponRepository _couponRepository;

        public CouponController(ICouponRepository couponRepository)
        {
            _couponRepository = couponRepository;
        }

        // GET: Admin/Coupon/Index
        public async Task<IActionResult> Index()
        {
            var coupons = await _couponRepository.GetActiveAsync();
            return View(coupons);
        }

        // GET: Admin/Coupon/Create
        public IActionResult Create()
        {
            var coupon = new Coupon
            {
                StartDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(1)
            };
            return View(coupon);
        }

        // POST: Admin/Coupon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                await _couponRepository.AddAsync(coupon);
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        // GET: Admin/Coupon/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null)
                return NotFound();

            return View(coupon);
        }

        // POST: Admin/Coupon/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Coupon coupon)
        {
            if (id != coupon.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                await _couponRepository.UpdateAsync(coupon);
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        // GET: Admin/Coupon/Delete/{id}
        public async Task<IActionResult> Delete(int id)
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null)
                return NotFound();

            return View(coupon);
        }

        // POST: Admin/Coupon/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _couponRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
