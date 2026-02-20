using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Affiliance_core.Entites
{
    public class NotificationPreference
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public NotificationType NotificationType { get; set; }

        public bool IsEmailEnabled { get; set; } = true;

        public bool IsPushEnabled { get; set; } = true;

        public bool IsInAppEnabled { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }

    public enum NotificationType
    {
        System,
        CampaignUpdate,
        ApplicationStatus,
        NewEarning,
        ComplaintUpdate,
        AiMatch,
        WithdrawalStatus,
        ReviewReceived,
        All
    }
}
