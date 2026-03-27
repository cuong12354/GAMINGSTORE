using GAMINGSTORE.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GAMINGSTORE.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // DANH SÁCH ĐƠN
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Order.ToListAsync();
            return View(orders);
        }

        // CHI TIẾT
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Order
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            return View(order);
        }

        // XÁC NHẬN THANH TOÁN
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            var order = await _context.Order.FindAsync(id);

            if (order != null)
            {
                order.Status = "Paid";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // GIAO HÀNG
        public async Task<IActionResult> Shipping(int id)
        {
            var order = await _context.Order.FindAsync(id);

            if (order != null)
            {
                order.Status = "Shipping";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}