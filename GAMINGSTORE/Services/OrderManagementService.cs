using GAMINGSTORE.Data;
using GAMINGSTORE.Models;
using Microsoft.EntityFrameworkCore;

namespace GAMINGSTORE.Services
{
    public class OrderManagementService : IOrderManagementService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderManagementService> _logger;

        private readonly List<string> _availableStatuses = new List<string>
        {
            "Pending",
            "Confirmed", 
            "Shipped",
            "Delivered",
            "Cancelled"
        };

        public OrderManagementService(ApplicationDbContext context, ILogger<OrderManagementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lấy tất cả đơn hàng
        /// </summary>
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy chi tiết đơn hàng
        /// </summary>
        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        /// <summary>
        /// Cập nhật trạng thái đơn hàng
        /// </summary>
        public async Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            try
            {
                // Validate status
                if (!_availableStatuses.Contains(newStatus))
                {
                    _logger.LogWarning($"Invalid status: {newStatus}");
                    return false;
                }

                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning($"Order not found: {orderId}");
                    return false;
                }

                var oldStatus = order.Status;
                order.Status = newStatus;

                // Set delivered date if status is Delivered
                if (newStatus == "Delivered" && order.DeliveredDate == null)
                {
                    order.DeliveredDate = DateTime.Now;
                }

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Order {orderId} status updated from {oldStatus} to {newStatus}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating order status: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lấy danh sách trạng thái có sẵn
        /// </summary>
        public async Task<List<string>> GetAvailableStatusesAsync()
        {
            return await Task.FromResult(_availableStatuses);
        }
    }
}
