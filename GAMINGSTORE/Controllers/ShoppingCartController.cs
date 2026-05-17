using GAMINGSTORE.Data;
using GAMINGSTORE.Extensions;
using GAMINGSTORE.Models;
using GAMINGSTORE.Repositories;
using GAMINGSTORE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class ShoppingCartController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILoyaltyService _loyaltyService;
    private readonly INotificationService _notificationService;

    public ShoppingCartController(ApplicationDbContext context,
    UserManager<ApplicationUser> userManager, IProductRepository
    productRepository, ICouponRepository couponRepository, ILoyaltyService loyaltyService, INotificationService notificationService)
    {
        _productRepository = productRepository;
        _couponRepository = couponRepository;
        _context = context;
        _userManager = userManager;
        _loyaltyService = loyaltyService;
        _notificationService = notificationService;
    }
    
    [Authorize]
    public IActionResult Checkout()
    {
        return View(new Order());
    }

    [Authorize]
[HttpPost]
public async Task<IActionResult> Checkout(Order order, string couponCode = "")
{
    var cart = HttpContext.Session.GetShoppingCart(User);
    if (cart == null || !cart.Items.Any())
    {
        return RedirectToAction("Index");
    }

    var user = await _userManager.GetUserAsync(User);
    if (user == null)
        return Unauthorized();

    if (string.IsNullOrEmpty(order.Notes))
        order.Notes = "";

    if (string.IsNullOrEmpty(order.CustomerName))
        order.CustomerName = user.UserName;

    if (string.IsNullOrEmpty(order.ShippingAddress))
        order.ShippingAddress = user.Address ?? "Not provided";

    order.UserId = user.Id;
    order.OrderDate = DateTime.UtcNow;
    order.Status = "PendingReview";

    if (string.IsNullOrEmpty(order.PaymentMethod))
        order.PaymentMethod = "COD";

    decimal baseTotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);

    // =========================
    // 1. ÁP COUPON TRƯỚC
    // =========================
    couponCode = couponCode?.Trim().ToUpper() ?? "";

    if (!string.IsNullOrEmpty(couponCode))
    {
        var coupon = await _couponRepository.GetByCodeAsync(couponCode);

        if (coupon != null &&
            coupon.IsActive &&
            coupon.StartDate <= DateTime.UtcNow &&
            coupon.ExpiryDate > DateTime.UtcNow &&
            baseTotalPrice >= coupon.MinimumOrderValue &&
            (coupon.MaxUsageCount <= 0 || coupon.CurrentUsageCount < coupon.MaxUsageCount))
        {
            decimal couponDiscount = 0;

            if (coupon.DiscountPercent > 0)
            {
                couponDiscount = Math.Round(baseTotalPrice * (coupon.DiscountPercent / 100));
            }
            else if (coupon.DiscountAmount > 0)
            {
                couponDiscount = coupon.DiscountAmount;
            }

            if (couponDiscount > baseTotalPrice)
            {
                couponDiscount = baseTotalPrice;
            }

            order.CouponCode = coupon.Code;
            order.DiscountAmount = couponDiscount;
        }
    }

    decimal priceAfterCoupon = baseTotalPrice - order.DiscountAmount;
    if (priceAfterCoupon < 0)
    {
        priceAfterCoupon = 0;
    }

    // =========================
    // 2. ÁP GIẢM GIÁ HỘI VIÊN SAU COUPON
    // =========================
    var memberTier = await _loyaltyService.GetUserMemberTierAsync(user.Id);

    if (memberTier != null && memberTier.DiscountPercentage > 0)
    {
        order.MemberDiscountPercentage = memberTier.DiscountPercentage;
        order.MemberDiscountAmount = Math.Round(priceAfterCoupon * (memberTier.DiscountPercentage / 100));
    }

    // =========================
    // 3. TỔNG TIỀN CUỐI
    // =========================
    order.TotalPrice = priceAfterCoupon - order.MemberDiscountAmount;

    if (order.TotalPrice < 0)
    {
        order.TotalPrice = 0;
    }

    // =========================
    // 4. CHI TIẾT ĐƠN HÀNG
    // =========================
    order.OrderDetails = new List<OrderDetail>();

    foreach (var cartItem in cart.Items)
    {
        var product = await _productRepository.GetByIdAsync(cartItem.ProductId);

        order.OrderDetails.Add(new OrderDetail
        {
            ProductId = cartItem.ProductId,
            Quantity = cartItem.Quantity,
            Price = cartItem.Price,
            ProductName = product?.Name ?? "Unknown Product"
        });
    }

    _context.Orders.Add(order);
    await _context.SaveChangesAsync();

    return RedirectToAction("OrderReview", new { orderId = order.Id });
}

    // ✨ OrderReview: Hiển thị trang xem trước đơn hàng
    [Authorize]
    public async Task<IActionResult> OrderReview(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return NotFound();

        return View(order);
    }

    // ✨ ConfirmPayment: Xác nhận thanh toán và hoàn tất đơn hàng
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ConfirmPayment(Order updatedOrder)
    {
        var order = await _context.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.Id == updatedOrder.Id);
        
        if (order == null)
            return NotFound();

        // ✨ Update order with latest information from form
        order.CustomerName = updatedOrder.CustomerName ?? order.CustomerName;
        order.Phone = updatedOrder.Phone;
        order.ShippingAddress = updatedOrder.ShippingAddress ?? order.ShippingAddress;
        order.Notes = updatedOrder.Notes;
        order.PaymentMethod = updatedOrder.PaymentMethod ?? order.PaymentMethod;

        // ✨ Trừ số lượng tồn kho (Inventory)
        foreach (var detail in order.OrderDetails)
        {
            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == detail.ProductId);
            if (inventory != null)
            {
                inventory.StockQuantity -= detail.Quantity;
                if (inventory.StockQuantity < 0) inventory.StockQuantity = 0;
                _context.Inventories.Update(inventory);
            }
        }

        // ✨ Update status to Confirmed
        order.Status = "Confirmed";
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();

        // ✨ Award Loyalty Points: 1 point per ₫1,000 spent
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            // Calculate points: 1 point per ₫1,000 (divide by 1000 and round down)
            int pointsEarned = (int)(order.TotalPrice / 1000);
            if (pointsEarned > 0)
            {
                // Add points to user account
                await _loyaltyService.AddPointsAsync(
                    user.Id,
                    pointsEarned,
                    $"Mua hàng - Đơn hàng #{order.Id}",
                    order.Id
                );

                // Upgrade tier if needed
                await _loyaltyService.UpdateMemberTierAsync(user.Id);

                // Store points earned in TempData for display
                TempData["PointsEarned"] = pointsEarned;
            }
        }

        // ✨ Clear shopping cart
        HttpContext.Session.RemoveShoppingCart(User);

        // ✨ Gửi Email Xác Nhận Đơn Hàng
        if (user != null && !string.IsNullOrEmpty(user.Email))
        {
            string emailSubject = $"[Gaming Store] Xác nhận đơn hàng #{order.Id}";
            string itemsHtml = string.Join("", order.OrderDetails.Select(od => 
                $"<tr><td style='padding:10px; border-bottom:1px solid #ddd;'>{od.ProductName}</td><td style='padding:10px; border-bottom:1px solid #ddd; text-align:center;'>{od.Quantity}</td><td style='padding:10px; border-bottom:1px solid #ddd; text-align:right;'>{od.Price:N0}₫</td></tr>"
            ));
            
            string emailBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #d92e2e;'>Cảm ơn bạn đã đặt hàng tại Gaming Store!</h2>
                <p>Xin chào <strong>{order.CustomerName}</strong>,</p>
                <p>Đơn hàng <strong>#{order.Id}</strong> của bạn đã được xác nhận thành công.</p>
                
                <h3 style='border-bottom: 2px solid #eee; padding-bottom: 10px;'>Chi tiết đơn hàng</h3>
                <table style='width: 100%; border-collapse: collapse;'>
                    <thead>
                        <tr style='background-color: #f9f9f9;'>
                            <th style='padding: 10px; text-align: left; border-bottom: 2px solid #ddd;'>Sản phẩm</th>
                            <th style='padding: 10px; text-align: center; border-bottom: 2px solid #ddd;'>Số lượng</th>
                            <th style='padding: 10px; text-align: right; border-bottom: 2px solid #ddd;'>Đơn giá</th>
                        </tr>
                    </thead>
                    <tbody>
                        {itemsHtml}
                    </tbody>
                    <tfoot>
                        <tr>
                            <td colspan='2' style='padding: 10px; text-align: right; font-weight: bold;'>Tổng tiền thanh toán:</td>
                            <td style='padding: 10px; text-align: right; font-weight: bold; color: #d92e2e; font-size: 18px;'>{order.TotalPrice:N0}₫</td>
                        </tr>
                    </tfoot>
                </table>
                
                <div style='margin-top: 20px; background-color: #f9f9f9; padding: 15px; border-radius: 5px;'>
                    <p style='margin: 5px 0;'><strong>Phương thức thanh toán:</strong> {order.PaymentMethod}</p>
                    <p style='margin: 5px 0;'><strong>Địa chỉ giao hàng:</strong> {order.ShippingAddress}</p>
                    <p style='margin: 5px 0;'><strong>SĐT:</strong> {order.Phone}</p>
                </div>
                
                <p style='margin-top: 20px; font-size: 14px; color: #666;'>Chúng tôi sẽ sớm giao hàng cho bạn. Cảm ơn bạn đã tin tưởng!</p>
            </div>
            ";

            await _notificationService.SendEmailAsync(user.Email, emailSubject, emailBody, user.Id);
        }

        // ✨ Redirect to OrderCompleted
        return RedirectToAction("OrderCompleted", new { orderId = order.Id });
    }

    // ✨ OrderCompleted: Trang xác nhận hoàn tất
    [Authorize]
    public async Task<IActionResult> OrderCompleted(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);
        
        if (order == null)
            return NotFound();

        return View(order);
    }
    // VIEW GIỎ HÀNG
    public IActionResult Index()
    {
        var cart = HttpContext.Session.GetShoppingCart(User);
        return View(cart);
    }

    [HttpGet]
    public IActionResult GetCartPartial()
    {
        var cart = HttpContext.Session.GetShoppingCart(User);
        return PartialView("_CartPartial", cart);
    }

    // THÊM VÀO GIỎ
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
    {
        if (quantity <= 0) quantity = 1;

        var product = await _productRepository.GetByIdAsync(productId);

        var cartItem = new CartItem
        {
            ProductId = productId,
            Name = product.Name,
            Price = product.Price,
            Quantity = quantity
        };

        var cart = HttpContext.Session.GetShoppingCart(User);

        cart.AddItem(cartItem);

        HttpContext.Session.SetShoppingCart(User, cart);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { success = true, totalQuantity = cart.GetTotalQuantity() });
        }

        return RedirectToAction("Index", "Home");
    }

    // XÓA
    public IActionResult RemoveFromCart(int productId)
    {
        var cart = HttpContext.Session.GetShoppingCart(User);

        if (cart.Items.Any())
        {
            cart.RemoveItem(productId);
            HttpContext.Session.SetShoppingCart(User, cart);
        }

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { success = true, totalQuantity = cart.GetTotalQuantity() });
        }

        return RedirectToAction("Index");
    }

    // TĂNG
    public IActionResult Increase(int productId)
    {
        var cart = HttpContext.Session.GetShoppingCart(User);

        if (cart.Items.Any())
        {
            cart.IncreaseQuantity(productId);
            HttpContext.Session.SetShoppingCart(User, cart);
        }

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { success = true, totalQuantity = cart.GetTotalQuantity() });
        }

        return RedirectToAction("Index");
    }

    // GIẢM
    public IActionResult Decrease(int productId)
    {
        var cart = HttpContext.Session.GetShoppingCart(User);

        if (cart.Items.Any())
        {
            cart.DecreaseQuantity(productId);
            HttpContext.Session.SetShoppingCart(User, cart);
        }

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { success = true, totalQuantity = cart.GetTotalQuantity() });
        }

        return RedirectToAction("Index");
    }

    // CLEAR
    public IActionResult ClearCart()
    {
        HttpContext.Session.RemoveShoppingCart(User);
        return RedirectToAction("Index");
    }
}
