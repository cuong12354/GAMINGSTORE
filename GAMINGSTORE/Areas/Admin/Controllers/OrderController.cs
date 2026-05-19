using GAMINGSTORE.Authorization;
using GAMINGSTORE.Data;
using GAMINGSTORE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GAMINGSTORE.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = PermissionConstants.OrderManage)]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // DANH SÁCH ĐƠN - Chỉ admin/staff có quyền OrderManage mới được xem
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }

        // CHI TIẾT - Kiểm tra quyền sở hữu hoặc quyền admin
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
                .Include(o => o.ApplicationUser)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            // Kiểm tra: User chỉ được xem đơn của chính mình, trừ khi có quyền OrderManage
            var hasOrderManagePermission = User.HasClaim(PermissionConstants.ClaimType, PermissionConstants.OrderManage);
            if (order.UserId != user.Id && !hasOrderManagePermission)
                return Forbid();

            return View(order);
        }

        // XÁC NHẬN THANH TOÁN - Chỉ admin/staff có quyền OrderManage
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order != null)
            {
                order.Status = "Paid";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // GIAO HÀNG - Chỉ admin/staff có quyền OrderManage
        public async Task<IActionResult> Shipping(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order != null)
            {
                order.Status = "Shipping";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}