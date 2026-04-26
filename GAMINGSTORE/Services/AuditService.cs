using GAMINGSTORE.Data;
using GAMINGSTORE.Models;
using Microsoft.EntityFrameworkCore;

namespace GAMINGSTORE.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditService> _logger;

        public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogActionAsync(string userId, string actionType, string entityName, int entityId,
            string? description = null, string? oldValues = null, string? newValues = null,
            string? ipAddress = null, string? userAgent = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    ActionType = actionType,
                    EntityName = entityName,
                    EntityId = entityId,
                    Description = description,
                    OldValues = oldValues,
                    NewValues = newValues,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    CreatedDate = DateTime.Now
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Audit logged: User {userId} performed {actionType} on {entityName} #{entityId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error logging audit: {ex.Message}");
            }
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(int? userId = null, string? entityName = null,
            int pageNumber = 1, int pageSize = 20)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(a => a.Id == userId.Value);
            }

            if (!string.IsNullOrEmpty(entityName))
            {
                query = query.Where(a => a.EntityName == entityName);
            }

            return await query
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetUserAuditLogsAsync(string userId, int pageNumber = 1, int pageSize = 20)
        {
            return await _context.AuditLogs
                .Where(a => a.UserId == userId)
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetEntityAuditLogsAsync(string entityName, int entityId)
        {
            return await _context.AuditLogs
                .Where(a => a.EntityName == entityName && a.EntityId == entityId)
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        public async Task<int> GetTotalAuditLogsCountAsync()
        {
            return await _context.AuditLogs.CountAsync();
        }

        public async Task ClearOldAuditLogsAsync(int daysToKeep = 90)
        {
            var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
            var oldLogs = _context.AuditLogs.Where(a => a.CreatedDate < cutoffDate);

            _context.AuditLogs.RemoveRange(oldLogs);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Cleared audit logs older than {cutoffDate}");
        }
    }
}
