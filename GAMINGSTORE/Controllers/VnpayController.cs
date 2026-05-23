using GAMINGSTORE.Data;
using GAMINGSTORE.Models;
using GAMINGSTORE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GAMINGSTORE.Controllers
{
    [Authorize]
    public class VnpayController : Controller
    {
        private readonly IVnpayService _vnpayService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly ILoyaltyService _loyaltyService;
        private readonly ILogger<VnpayController> _logger;

        public VnpayController(
            IVnpayService vnpayService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService,
            ILoyaltyService loyaltyService,
            ILogger<VnpayController> logger)
        {
            _vnpayService = vnpayService;
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
            _loyaltyService = loyaltyService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo URL thanh toán VNPay
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePaymentUrl(int orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return NotFound(new { message = "Đơn hàng không tồn tại" });
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null || order.UserId != user.Id)
                {
                    return Unauthorized(new { message = "Bạn không có quyền thanh toán đơn hàng này" });
                }

                var returnUrl = Url.Action("PaymentReturn", "Vnpay", null, Request.Scheme);
                var orderInfo = $"Thanh toán đơn hàng #{orderId}";
                
                var paymentUrl = _vnpayService.CreatePaymentUrl(
                    orderId,
                    order.TotalPrice,
                    orderInfo,
                    returnUrl
                );

                _logger.LogInformation($"VNPay payment URL created for order {orderId}");
                return Ok(new { paymentUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating VNPay payment URL: {ex.Message}");
                return BadRequest(new { message = "Lỗi tạo URL thanh toán" });
            }
        }

        /// <summary>
        /// Xử lý kết quả thanh toán từ VNPay
        /// </summary>
        [AllowAnonymous]
        public async Task<IActionResult> PaymentReturn()
        {
            try
            {
                var (success, message, orderId) = _vnpayService.ProcessPaymentReturn(Request.Query);

                if (!orderId.HasValue)
                {
                    return BadRequest(new { message = "Không tìm thấy đơn hàng" });
                }

                var order = await _context.Orders
                    .Include(o => o.ApplicationUser)
                    .FirstOrDefaultAsync(o => o.Id == orderId.Value);

                if (order == null)
                {
                    return NotFound(new { message = "Đơn hàng không tồn tại" });
                }

                if (success)
                {
                    // Cập nhật trạng thái đơn hàng
                    order.Status = "Confirmed";
                    order.PaymentMethod = "VNPay";
                    await _context.SaveChangesAsync();

                    // Thêm điểm thưởng
                    var points = (int)(order.TotalPrice / 1000); // 1 điểm = 1000 VNĐ
                    if (points > 0)
                    {
                        await _loyaltyService.AddPointsAsync(
                            order.UserId,
                            points,
                            $"Mua hàng đơn #{orderId.Value}",
                            orderId.Value
                        );
                    }

                    // Gửi email thông báo
                    await _notificationService.SendEmailAsync(
                        order.ApplicationUser.Email,
                        "Thanh toán thành công",
                        $"<p>Đơn hàng #{orderId.Value} của bạn đã được thanh toán thành công.</p>" +
                        $"<p>Tổng tiền: {order.TotalPrice:C0}</p>" +
                        $"<p>Bạn đã nhận được {points} điểm thưởng.</p>",
                        order.UserId
                    );

                    _logger.LogInformation($"Payment successful for order {orderId.Value}");
                    return RedirectToAction("PaymentSuccess", new { orderId = orderId.Value });
                }
                else
                {
                    _logger.LogWarning($"Payment failed for order {orderId.Value}: {message}");
                    return RedirectToAction("PaymentFailed", new { orderId = orderId.Value, message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing VNPay return: {ex.Message}");
                return BadRequest(new { message = "Lỗi xử lý thanh toán" });
            }
        }

        /// <summary>
        /// Trang thành công
        /// </summary>
        [AllowAnonymous]
        public async Task<IActionResult> PaymentSuccess(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        /// <summary>
        /// Trang thất bại
        /// </summary>
        [AllowAnonymous]
        public IActionResult PaymentFailed(int orderId, string message)
        {
            ViewBag.OrderId = orderId;
            ViewBag.Message = message;
            return View();
        }
    }
}
