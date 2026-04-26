using GAMINGSTORE.Models;

namespace GAMINGSTORE.Services
{
    public interface IOrderManagementService
    {
        Task<List<Order>> GetAllOrdersAsync();
        Task<Order> GetOrderByIdAsync(int orderId);
        Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus);
        Task<List<string>> GetAvailableStatusesAsync();
    }
}
