using GAMINGSTORE.Models;
using GAMINGSTORE.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GAMINGSTORE.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly IProductRepository _productRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(IWishlistRepository wishlistRepository, IProductRepository productRepository, UserManager<ApplicationUser> userManager)
        {
            _wishlistRepository = wishlistRepository;
            _productRepository = productRepository;
            _userManager = userManager;
        }

        // GET: Wishlist/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var wishlist = await _wishlistRepository.GetUserWishlistAsync(user.Id);
            var products = new List<Product>();

            foreach (var item in wishlist)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product != null)
                    products.Add(product);
            }

            return View(products);
        }

        // POST: Wishlist/Add/{productId}
        [HttpPost]
        public async Task<IActionResult> Add(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return NotFound();

            var wishlist = new Wishlist
            {
                UserId = user.Id,
                ProductId = productId,
                AddedDate = DateTime.UtcNow
            };

            await _wishlistRepository.AddAsync(wishlist);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Ok(new { success = true, message = "Đã thêm vào danh sách yêu thích" });

            return RedirectToAction("Index");
        }

        // POST: Wishlist/Remove/{id}
        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var wishlist = await Task.FromResult(new Wishlist { Id = id });
            var existing = (await _wishlistRepository.GetUserWishlistAsync(user.Id))
                .FirstOrDefault(w => w.Id == id);

            if (existing == null)
                return NotFound();

            await _wishlistRepository.RemoveAsync(id);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Ok(new { success = true, message = "Đã xóa khỏi danh sách yêu thích" });

            return RedirectToAction("Index");
        }

        // GET: Wishlist/IsInWishlist/{productId}
        [HttpGet]
        public async Task<IActionResult> IsInWishlist(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { inWishlist = false });

            var isInWishlistValue = await _wishlistRepository.IsInWishlistAsync(user.Id, productId);
            return Json(new { inWishlist = isInWishlistValue });
        }
    }
}
