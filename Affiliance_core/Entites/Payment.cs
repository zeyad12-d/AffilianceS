using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Affiliance_core.Entites
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public int? CampaignId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        public PaymentType Type { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [MaxLength(255)]
        public string? TransactionId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("CampaignId")]
        public virtual Campaign? Campaign { get; set; }
    }

    public enum PaymentType
    {
        Commission,
        Withdrawal,
        Refund,
        Bonus,
        Adjustment
    }

    public enum PaymentStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Cancelled
    }
}
