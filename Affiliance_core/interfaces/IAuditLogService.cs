using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.AuditDto;

namespace Affiliance_core.interfaces
{
    public interface IAuditLogService
    {
        // ========================================
        // Audit Logging
        // ========================================
        
        /// <summary>
        /// Log an action (internal use by other services)
        /// </summary>
        Task<ApiResponse<bool>> LogActionAsync(int? userId, string action, string? entityType = null, int? entityId = null, 
            string? oldValues = null, string? newValues = null, string? ipAddress = null, string? userAgent = null);
        
        /// <summary>
        /// Get audit logs with filtering
        /// </summary>
        Task<ApiResponse<PagedResult<AuditLogDto>>> GetAuditLogsAsync(AuditLogFilterDto filter);
        
        /// <summary>
        /// Get activity history for a user
        /// </summary>
        Task<ApiResponse<List<ActivityHistoryDto>>> GetActivityHistoryAsync(int userId, int days = 30);
        
        /// <summary>
        /// Get activity history for an entity
        /// </summary>
        Task<ApiResponse<List<AuditLogDto>>> GetEntityHistoryAsync(string entityType, int entityId);
    }
}
