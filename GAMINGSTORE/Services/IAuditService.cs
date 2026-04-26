namespace GAMINGSTORE.Services
{
    public interface IAuditService
    {
        Task LogActionAsync(string userId, string actionType, string entityName, int entityId, 
            string? description = null, string? oldValues = null, string? newValues = null,
            string? ipAddress = null, string? userAgent = null);

        Task<List<Models.AuditLog>> GetAuditLogsAsync(int? userId = null, string? entityName = null, 
            int pageNumber = 1, int pageSize = 20);

        Task<List<Models.AuditLog>> GetUserAuditLogsAsync(string userId, int pageNumber = 1, int pageSize = 20);

        Task<List<Models.AuditLog>> GetEntityAuditLogsAsync(string entityName, int entityId);

        Task<int> GetTotalAuditLogsCountAsync();

        Task ClearOldAuditLogsAsync(int daysToKeep = 90);
    }
}
