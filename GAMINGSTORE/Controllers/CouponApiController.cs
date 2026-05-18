using GAMINGSTORE.Models;
using GAMINGSTORE.Repositories;
using GAMINGSTORE.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace GAMINGSTORE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CouponController : ControllerBase
    {
        private readonly ICouponRepository _couponRepository;
        private readonly ApplicationDbContext _context;

        public CouponController(ICouponRepository couponRepository, ApplicationDbContext context)
        {
            _couponRepository = couponRepository;
            _context = context;
        }

        // ✨ API: Validate coupon code
        // ✨ API: Validate coupon code
[HttpPost("validate")]
[AllowAnonymous]
public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponRequest request)
{
    if (string.IsNullOrEmpty(request?.Code))
    {
        return BadRequest(new { success = false, message = "Mã giảm giá không hợp lệ" });
    }

    try
    {
        var coupon = await _couponRepository.GetByCodeAsync(request.Code);
        var now = DateTime.UtcNow; // Lấy thời gian hiện tại để so sánh

        if (coupon == null)
        {
            return Ok(new 
            { 
                success = false, 
                message = "Mã giảm giá không tồn tại",
                code = request.Code 
            });
        }

        // 1. Kiểm tra mã có đang được kích hoạt không
        if (!coupon.IsActive)
        {
            return Ok(new { success = false, message = "Mã giảm giá này đang tạm khóa", code = request.Code });
        }

        // 2. Kiểm tra thời gian bắt đầu
        if (coupon.StartDate > now)
        {
            return Ok(new { success = false, message = "Mã giảm giá chưa đến thời gian áp dụng", code = request.Code });
        }

        // 3. Kiểm tra thời gian hết hạn (code cũ của bạn)
        if (coupon.ExpiryDate < now)
        {
            return Ok(new { success = false, message = "Mã giảm giá đã hết hạn", code = request.Code });
        }

        // 4. KIỂM TRA LƯỢT SỬ DỤNG (Chống spam vượt quá số lượng)
        if (coupon.CurrentUsageCount >= coupon.MaxUsageCount)
        {
            return Ok(new { success = false, message = "Mã giảm giá đã hết lượt sử dụng", code = request.Code });
        }

        // Check minimum order value
        decimal totalPrice = request.TotalPrice ?? 0;
        if (totalPrice < coupon.MinimumOrderValue)
        {
            decimal minRequired = coupon.MinimumOrderValue;
            return Ok(new 
            { 
                success = false, 
                message = $"Đơn hàng phải tối thiểu {minRequired.ToString("F0")} ₫",
                code = request.Code 
            });
        }

        // Calculate discount
        decimal discountAmount = 0;
        if (coupon.DiscountPercent > 0)
        {
            discountAmount = Math.Round(totalPrice * (coupon.DiscountPercent / 100));
        }
        else if (coupon.DiscountAmount > 0)
        {
            discountAmount = coupon.DiscountAmount;
        }

        return Ok(new 
        { 
            success = true, 
            message = "Áp dụng mã giảm giá thành công!",
            code = coupon.Code,
            discountAmount = discountAmount,
            discountPercent = coupon.DiscountPercent,
            discountFixed = coupon.DiscountAmount
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
    }
}

        // ✨ Test coupon codes for demo
        [HttpPost("test")]
        [AllowAnonymous]
        public IActionResult TestCoupon([FromBody] ValidateCouponRequest request)
        {
            if (string.IsNullOrEmpty(request?.Code))
            {
                return BadRequest(new { success = false, message = "Mã giảm giá không hợp lệ" });
            }

            var code = request.Code.ToUpper();
            var totalPrice = request.TotalPrice ?? 0;

            // Test codes for demo
            if (code == "SUMMER2024")
            {
                var discount = Math.Round(totalPrice * 0.2m);
                return Ok(new 
                { 
                    success = true, 
                    message = "Áp dụng mã SUMMER2024 thành công! Giảm 20%",
                    code = "SUMMER2024",
                    discountAmount = discount,
                    discountPercent = 20
                });
            }

            if (code == "SALE50")
            {
                var discount = Math.Round(totalPrice * 0.5m);
                return Ok(new 
                { 
                    success = true, 
                    message = "Áp dụng mã SALE50 thành công! Giảm 50%",
                    code = "SALE50",
                    discountAmount = discount,
                    discountPercent = 50
                });
            }

            if (code == "SAVE100K")
            {
                return Ok(new 
                { 
                    success = true, 
                    message = "Áp dụng mã SAVE100K thành công! Giảm 100.000 ₫",
                    code = "SAVE100K",
                    discountAmount = 100000,
                    discountFixed = 100000
                });
            }

            return Ok(new 
            { 
                success = false, 
                message = "Mã giảm giá không hợp lệ. Thử: SUMMER2024, SALE50, SAVE100K",
                code = code 
            });
        }
    }

    public class ValidateCouponRequest
    {
        public string Code { get; set; }
        public decimal? TotalPrice { get; set; }
    }
}
