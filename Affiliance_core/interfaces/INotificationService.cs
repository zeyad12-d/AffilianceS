using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.NotificationDto;

namespace Affiliance_core.interfaces
{
    public interface INotificationService
    {
        // ========================================
        // Notification Management
        // ========================================
        
        /// <summary>
        /// Get user's notifications with filtering
        /// </summary>
        Task<ApiResponse<PagedResult<NotificationListDto>>> GetMyNotificationsAsync(int userId, NotificationFilterDto filter);
        
        /// <summary>
        /// Get notification summary (total, unread count)
        /// </summary>
        Task<ApiResponse<NotificationSummaryDto>> GetNotificationSummaryAsync(int userId);
        
        /// <summary>
        /// Mark notification as read
        /// </summary>
        Task<ApiResponse<bool>> MarkAsReadAsync(int notificationId, int userId);
        
        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        Task<ApiResponse<bool>> MarkAllAsReadAsync(int userId);
        
        /// <summary>
        /// Delete notification
        /// </summary>
        Task<ApiResponse<bool>> DeleteNotificationAsync(int notificationId, int userId);

        // ========================================
        // Notification Preferences
        // ========================================
        
        /// <summary>
        /// Get user's notification preferences
        /// </summary>
        Task<ApiResponse<List<NotificationPreferenceDto>>> GetNotificationPreferencesAsync(int userId);
        
        /// <summary>
        /// Update notification preference
        /// </summary>
        Task<ApiResponse<NotificationPreferenceDto>> UpdateNotificationPreferenceAsync(int userId, UpdateNotificationPreferenceDto dto);

        // ========================================
        // Internal Methods
        // ========================================
        
        /// <summary>
        /// Create a new notification (internal use by other services)
        /// </summary>
        Task<ApiResponse<bool>> CreateNotificationAsync(int userId, string title, string body, string type, int? relatedId = null);
    }
}
