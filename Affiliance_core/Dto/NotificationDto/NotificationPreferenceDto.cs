using Affiliance_core.Entites;

namespace Affiliance_core.Dto.NotificationDto
{
    public class NotificationPreferenceDto
    {
        public int Id { get; set; }
        public NotificationType NotificationType { get; set; }
        public string NotificationTypeDisplay { get; set; } = null!;
        public bool IsEmailEnabled { get; set; }
        public bool IsPushEnabled { get; set; }
        public bool IsInAppEnabled { get; set; }
    }
}
