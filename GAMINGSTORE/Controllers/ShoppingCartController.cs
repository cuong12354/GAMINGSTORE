using GAMINGSTORE.Data;
using GAMINGSTORE.Extensions;
using GAMINGSTORE.Models;
using GAMINGSTORE.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class ShoppingCartController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    public ShoppingCartController(ApplicationDbContext context,
    UserManager<ApplicationUser> userManager, IProductRepository
    productRepository)
    {
        _productRepository = productRepository;
        _context = context;
        _userManager = userManager;
    }
    public IActionResult Checkout()
    {
        return View(new Order());
    }

    [HttpPost]
    public async Task<IActionResult> Checkout(Order order)
    {
        var cart = HttpContext.Session.GetShoppingCart(User);
        if (cart == null || !cart.Items.Any())
        {
            // Xử lý giỏ hàng trống...
            return RedirectToAction("Index");
        }
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        if (string.IsNullOrEmpty(order.Notes))
            order.Notes = "";

        if (string.IsNullOrEmpty(order.CustomerName))
            order.CustomerName = user.UserName;

        order.UserId = user.Id;
        order.OrderDate = DateTime.UtcNow;
        order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);

        order.Status = "Pending"; // hoặc "Đang xử lý"
        order.OrderDetails = cart.Items.Select(i => new OrderDetail
        {
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            Price = i.Price
        }).ToList();
        _context.Order.Add(order);
        await _context.SaveChangesAsync();
        HttpContext.Session.RemoveShoppingCart(User);
        return View("OrderCompleted", order.Id); // Trang xác nhận hoàn thành đơn hàng
        }
    // VIEW GIỎ HÀNG
    public IActionResult Index()
    {
        var cart = HttpContext.Session.GetShoppingCart(User);
        return View(cart);
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

        return RedirectToAction("Index");
    }

    // CLEAR
    public IActionResult ClearCart()
    {
        HttpContext.Session.RemoveShoppingCart(User);
        return RedirectToAction("Index");
    }
}