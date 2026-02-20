using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Affiliance_core.Entites
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? EntityType { get; set; }

        public int? EntityId { get; set; }

        [MaxLength(2000)]
        public string? OldValues { get; set; } // JSON

        [MaxLength(2000)]
        public string? NewValues { get; set; } // JSON

        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
