namespace Affiliance_core.Dto.NotificationDto
{
    public class NotificationSummaryDto
    {
        public int TotalNotifications { get; set; }
        public int UnreadCount { get; set; }
        public DateTime? LastNotificationDate { get; set; }
    }
}
