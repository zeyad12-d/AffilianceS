using Affiliance_core.Entites;
using System.ComponentModel.DataAnnotations;

namespace Affiliance_core.Dto.NotificationDto
{
    public class UpdateNotificationPreferenceDto
    {
        [Required(ErrorMessage = "Notification type is required")]
        public NotificationType NotificationType { get; set; }

        public bool? IsEmailEnabled { get; set; }

        public bool? IsPushEnabled { get; set; }

        public bool? IsInAppEnabled { get; set; }
    }
}
