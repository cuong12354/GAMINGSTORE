using GAMINGSTORE.Data;
using GAMINGSTORE.Extensions;
using GAMINGSTORE.Models;
using GAMINGSTORE.Repositories;
using GAMINGSTORE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class ShoppingCartController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILoyaltyService _loyaltyService;
    public ShoppingCartController(ApplicationDbContext context,
    UserManager<ApplicationUser> userManager, IProductRepository
    productRepository, ICouponRepository couponRepository, ILoyaltyService loyaltyService)
    {
        _productRepository = productRepository;
        _couponRepository = couponRepository;
        _context = context;
        _userManager = userManager;
        _loyaltyService = loyaltyService;
    }
    public IActionResult Checkout()
    {
        return View(new Order());
    }

    [HttpPost]
    public async Task<IActionResult> Checkout(Order order, string couponCode = "")
    {
        var cart = HttpContext.Session.GetShoppingCart(User);
        if (cart == null || !cart.Items.Any())
        {
            // X? l� gi? h�ng tr?ng...
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
        order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
        order.Status = "Pending";
        
        // ✨ Set default PaymentMethod if not provided
        if (string.IsNullOrEmpty(order.PaymentMethod))
            order.PaymentMethod = "COD";
        
        // ✨ Apply coupon discount if provided
        if (!string.IsNullOrEmpty(couponCode))
        {
            var coupon = await _couponRepository.GetByCodeAsync(couponCode);
            if (coupon != null && coupon.IsActive && 
                coupon.StartDate <= DateTime.UtcNow && coupon.ExpiryDate > DateTime.UtcNow &&
                order.TotalPrice >= coupon.MinimumOrderValue)
            {
                decimal discountAmount = 0;
                if (coupon.DiscountPercent > 0)
                {
                    discountAmount = Math.Round(order.TotalPrice * (coupon.DiscountPercent / 100));
                }
                else if (coupon.DiscountAmount > 0)
                {
                    discountAmount = coupon.DiscountAmount;
                }
                
                order.CouponCode = couponCode;
                order.DiscountAmount = discountAmount;
                order.TotalPrice = order.TotalPrice - discountAmount;
            }
        }
        
        // ✨ Load products for OrderDetails
        order.OrderDetails = new List<OrderDetail>();
        foreach (var cartItem in cart.Items)
        {
            var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
            order.OrderDetails.Add(new OrderDetail
            {
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,
                Price = cartItem.Price,
                ProductName = product?.Name ?? "Unknown Product"  // ✨ Store product name as backup
            });
        }
        
        // ✨ Save order with PendingReview status
        order.Status = "PendingReview";
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        
        // ✨ Redirect to OrderReview page instead of OrderCompleted
        return RedirectToAction("OrderReview", new { orderId = order.Id });
    }

    // ✨ OrderReview: Hiển thị trang xem trước đơn hàng
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
    [HttpPost]
    public async Task<IActionResult> ConfirmPayment(Order updatedOrder)
    {
        var order = await _context.Orders.FindAsync(updatedOrder.Id);
        
        if (order == null)
            return NotFound();

        // ✨ Update order with latest information from form
        order.CustomerName = updatedOrder.CustomerName ?? order.CustomerName;
        order.Phone = updatedOrder.Phone;
        order.ShippingAddress = updatedOrder.ShippingAddress ?? order.ShippingAddress;
        order.Notes = updatedOrder.Notes;
        order.PaymentMethod = updatedOrder.PaymentMethod ?? order.PaymentMethod;

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

        // ✨ Redirect to OrderCompleted
        return RedirectToAction("OrderCompleted", new { orderId = order.Id });
    }

    // ✨ OrderCompleted: Trang xác nhận hoàn tất
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
    // VIEW GI? H�NG
    public IActionResult Index()
    {
        var cart = HttpContext.Session.GetShoppingCart(User);
        return View(cart);
    }

    // TH�M V�O GI?
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

        return RedirectToAction("Index", "Home");
    }

    // X�A
    public IActionResult RemoveFromCart(int productId)
    {
        var cart = HttpContext.Session.GetShoppingCart(User);

        if (cart.Items.Any())
        {
            cart.RemoveItem(productId);
            HttpContext.Session.SetShoppingCart(User, cart);
        }

        return RedirectToAction("Index");
    }

    // TANG
    public IActionResult Increase(int productId)
    {
        var cart = HttpContext.Session.GetShoppingCart(User);

        if (cart.Items.Any())
        {
            cart.IncreaseQuantity(productId);
            HttpContext.Session.SetShoppingCart(User, cart);
        }

        return RedirectToAction("Index");
    }

    // GI?M
    public IActionResult Decrease(int productId)
    {
        var cart = HttpContext.Session.GetShoppingCart(User);

        if (cart.Items.Any())
        {
            cart.DecreaseQuantity(productId);
            HttpContext.Session.SetShoppingCart(User, cart);
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
