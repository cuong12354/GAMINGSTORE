using GAMINGSTORE.Models;

namespace GAMINGSTORE.Services
{
    public interface IReturnService
    {
        /// <summary>
        /// Create a new return request
        /// </summary>
        Task<ReturnRequest?> CreateReturnRequestAsync(
            string userId,
            int orderId,
            string reason,
            decimal returnAmount
        );

        /// <summary>
        /// Get all return requests for a user
        /// </summary>
        Task<List<ReturnRequest>> GetUserReturnRequestsAsync(string userId);

        /// <summary>
        /// Get return request by ID
        /// </summary>
        Task<ReturnRequest?> GetReturnRequestByIdAsync(int returnRequestId);

        /// <summary>
        /// Get all pending return requests (admin)
        /// </summary>
        Task<List<ReturnRequest>> GetPendingReturnRequestsAsync();

        /// <summary>
        /// Update return request status (admin)
        /// </summary>
        Task<bool> UpdateReturnStatusAsync(
            int returnRequestId,
            string status,
            string? adminNotes = null
        );

        /// <summary>
        /// Calculate refund amount for order
        /// </summary>
        Task<decimal> CalculateRefundAmountAsync(int orderId);

        /// <summary>
        /// Check if order can be returned (within 30 days)
        /// </summary>
        Task<bool> CanReturnOrderAsync(int orderId);

        /// <summary>
        /// Get return statistics for dashboard
        /// </summary>
        Task<Dictionary<string, int>> GetReturnStatisticsAsync();
    }
}
