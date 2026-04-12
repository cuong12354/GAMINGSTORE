using GAMINGSTORE.Data;
using GAMINGSTORE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace GAMINGSTORE.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Payment/BankTransferQR
        public async Task<IActionResult> BankTransferQR(int? orderId)
        {
            if (orderId == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == user.Id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // GET: Payment/PaymentSlip
        public async Task<IActionResult> PaymentSlip(int? orderId)
        {
            if (orderId == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == user.Id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // POST: Payment/ConfirmPayment
        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == user.Id);

            if (order == null)
                return NotFound();

            order.IsPaid = true;
            order.Status = "Confirmed";
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "OrderHistory", new { id = orderId });
        }
    }
}
