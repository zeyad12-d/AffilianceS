using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Affiliance_core.Entites
{
    public class PaymentMethod
    {
        [Key]
        public int Id { get; set; }

        public int MarketerId { get; set; }

        public PaymentMethodType Type { get; set; }

        [Required]
        [MaxLength(1000)]
        public string AccountDetails { get; set; } = string.Empty; // JSON string with account info

        [MaxLength(100)]
        public string? AccountHolderName { get; set; }

        public bool IsDefault { get; set; } = false;

        public bool IsVerified { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? VerifiedAt { get; set; }

        // Navigation Properties
        [ForeignKey("MarketerId")]
        public virtual Marketer Marketer { get; set; }

        public virtual ICollection<WithdrawalRequest> WithdrawalRequests { get; set; } = new List<WithdrawalRequest>();
    }

    public enum PaymentMethodType
    {
        BankAccount,
        Ewallet,
        Paypal,
        MobileMoney,
        Other
    }
}
