using GAMINGSTORE.Models;
using GAMINGSTORE.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GAMINGSTORE.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IProductRepository _productRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewController(IReviewRepository reviewRepository, IProductRepository productRepository, UserManager<ApplicationUser> userManager)
        {
            _reviewRepository = reviewRepository;
            _productRepository = productRepository;
            _userManager = userManager;
        }

        // GET: Review/GetForProduct/{productId}
        [AllowAnonymous]
        public async Task<IActionResult> GetForProduct(int productId)
        {
            var reviews = await _reviewRepository.GetByProductIdAsync(productId);
            return PartialView("_ReviewsList", reviews);
        }

        // GET: Review/Create/{productId}
        public async Task<IActionResult> Create(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var model = new ProductReview
            {
                ProductId = productId
            };
            return View(model);
        }

        // POST: Review/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductReview review)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            review.UserId = user.Id;
            review.CreatedDate = DateTime.UtcNow;

            // Check if user already reviewed this product
            var existingReview = (await _reviewRepository.GetByProductIdAsync(review.ProductId))
                .FirstOrDefault(r => r.UserId == user.Id);

            if (existingReview != null)
            {
                ModelState.AddModelError("", "Bạn đã đánh giá sản phẩm này rồi");
                return View(review);
            }

            await _reviewRepository.AddAsync(review);

            // Update product rating
            var avgRating = await _reviewRepository.GetAverageRatingAsync(review.ProductId);
            var reviewCount = await _reviewRepository.GetReviewCountAsync(review.ProductId);

            var product = await _productRepository.GetByIdAsync(review.ProductId);
            if (product != null)
            {
                product.AverageRating = (decimal)avgRating;
                product.ReviewCount = reviewCount;
            }

            return RedirectToAction("Details", "Product", new { id = review.ProductId });
        }

        // POST: Review/Delete/{id}
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (review.UserId != user.Id && !User.IsInRole("Admin"))
                return Forbid();

            var productId = review.ProductId;
            await _reviewRepository.DeleteAsync(id);

            // Update product rating
            var avgRating = await _reviewRepository.GetAverageRatingAsync(productId);
            var reviewCount = await _reviewRepository.GetReviewCountAsync(productId);

            var product = await _productRepository.GetByIdAsync(productId);
            if (product != null)
            {
                product.AverageRating = (decimal)avgRating;
                product.ReviewCount = reviewCount;
            }

            return RedirectToAction("Details", "Product", new { id = productId });
        }

        // POST: Review/MarkHelpful/{id}
        [HttpPost]
        public async Task<IActionResult> MarkHelpful(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
                return NotFound();

            review.HelpfulCount++;
            await _reviewRepository.UpdateAsync(review);

            return Ok(new { helpful = review.HelpfulCount });
        }
    }
}
