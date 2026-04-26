using GAMINGSTORE.Data;
using GAMINGSTORE.Models;
using Microsoft.EntityFrameworkCore;

namespace GAMINGSTORE.Services
{
    public class ReturnService : IReturnService
    {
        private readonly ApplicationDbContext _context;
        private const int RETURN_WINDOW_DAYS = 30;

        public ReturnService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create a new return request with validation
        /// </summary>
        public async Task<ReturnRequest?> CreateReturnRequestAsync(
            string userId,
            int orderId,
            string reason,
            decimal returnAmount
        )
        {
            try
            {
                // Verify order exists and belongs to user
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

                if (order == null)
                    return null;

                // Check if order can be returned
                if (!await CanReturnOrderAsync(orderId))
                    return null;

                // Check if return already exists for this order
                var existingReturn = await _context.ReturnRequests
                    .FirstOrDefaultAsync(r => r.OrderId == orderId);

                if (existingReturn != null)
                    return null;

                // Calculate refund amount
                decimal refundAmount = await CalculateRefundAmountAsync(orderId);
                if (refundAmount <= 0)
                    refundAmount = returnAmount;

                var returnRequest = new ReturnRequest
                {
                    OrderId = orderId,
                    UserId = userId,
                    Reason = reason,
                    ReturnAmount = refundAmount,
                    Status = "Pending",
                    RequestDate = DateTime.Now,
                    Order = order,
                    User = null
                };

                _context.ReturnRequests.Add(returnRequest);
                await _context.SaveChangesAsync();

                return returnRequest;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Lỗi tạo return request: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get all return requests for a user
        /// </summary>
        public async Task<List<ReturnRequest>> GetUserReturnRequestsAsync(string userId)
        {
            return await _context.ReturnRequests
                .Where(r => r.UserId == userId)
                .Include(r => r.Order)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get return request by ID
        /// </summary>
        public async Task<ReturnRequest?> GetReturnRequestByIdAsync(int returnRequestId)
        {
            return await _context.ReturnRequests
                .Include(r => r.Order)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == returnRequestId);
        }

        /// <summary>
        /// Get all pending return requests (admin)
        /// </summary>
        public async Task<List<ReturnRequest>> GetPendingReturnRequestsAsync()
        {
            return await _context.ReturnRequests
                .Where(r => r.Status == "Pending")
                .Include(r => r.Order)
                .Include(r => r.User)
                .OrderBy(r => r.RequestDate)
                .ToListAsync();
        }

        /// <summary>
        /// Update return request status (admin)
        /// </summary>
        public async Task<bool> UpdateReturnStatusAsync(
            int returnRequestId,
            string status,
            string? adminNotes = null
        )
        {
            try
            {
                var returnRequest = await _context.ReturnRequests
                    .FirstOrDefaultAsync(r => r.Id == returnRequestId);

                if (returnRequest == null)
                    return false;

                // Validate status
                var validStatuses = new[] { "Pending", "Approved", "Rejected", "Completed" };
                if (!validStatuses.Contains(status))
                    return false;

                returnRequest.Status = status;
                if (adminNotes != null)
                    returnRequest.AdminNotes = adminNotes;

                _context.ReturnRequests.Update(returnRequest);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Lỗi cập nhật return status: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Calculate refund amount for order (includes loyalty points deduction)
        /// </summary>
        public async Task<decimal> CalculateRefundAmountAsync(int orderId)
        {
            try
            {
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    return 0;

                // Refund amount = Total price - discount (if any)
                decimal refundAmount = order.TotalPrice + order.DiscountAmount;

                // Could apply restocking fee (10%) if needed
                // refundAmount = refundAmount * 0.9m;

                return refundAmount;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Lỗi tính toán refund: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Check if order can be returned (within 30 days)
        /// </summary>
        public async Task<bool> CanReturnOrderAsync(int orderId)
        {
            try
            {
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    return false;

                // Only allow return within RETURN_WINDOW_DAYS
                var daysSinceOrder = (DateTime.Now - order.OrderDate).Days;
                if (daysSinceOrder > RETURN_WINDOW_DAYS)
                    return false;

                // Only allow return if order status is Delivered or Completed
                if (order.Status != "Delivered" && order.Status != "Completed")
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Lỗi kiểm tra return: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get return statistics for dashboard
        /// </summary>
        public async Task<Dictionary<string, int>> GetReturnStatisticsAsync()
        {
            try
            {
                var stats = new Dictionary<string, int>();

                stats["Total"] = await _context.ReturnRequests.CountAsync();
                stats["Pending"] = await _context.ReturnRequests.CountAsync(r => r.Status == "Pending");
                stats["Approved"] = await _context.ReturnRequests.CountAsync(r => r.Status == "Approved");
                stats["Rejected"] = await _context.ReturnRequests.CountAsync(r => r.Status == "Rejected");
                stats["Completed"] = await _context.ReturnRequests.CountAsync(r => r.Status == "Completed");

                return stats;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Lỗi lấy thống kê return: {ex.Message}");
                return new Dictionary<string, int>();
            }
        }
    }
}
