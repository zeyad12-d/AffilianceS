using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Affiliance_core.Entites
{
    public class WithdrawalRequest
    {
        [Key]
        public int Id { get; set; }

        public int MarketerId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        public int PaymentMethodId { get; set; }

        public WithdrawalStatus Status { get; set; } = WithdrawalStatus.Pending;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; }

        public int? ProcessedBy { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        [MaxLength(500)]
        public string? AdminNotes { get; set; }

        // Navigation Properties
        [ForeignKey("MarketerId")]
        public virtual Marketer Marketer { get; set; }

        [ForeignKey("PaymentMethodId")]
        public virtual PaymentMethod PaymentMethod { get; set; }

        [ForeignKey("ProcessedBy")]
        public virtual User? ProcessedByUser { get; set; }
    }

    public enum WithdrawalStatus
    {
        Pending,
        Approved,
        Rejected,
        Processing,
        Completed,
        Failed
    }
}
