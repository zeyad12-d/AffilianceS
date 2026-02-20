using Affiliance_core.Entites;
using System.ComponentModel.DataAnnotations;

namespace Affiliance_core.Dto.PaymentDto
{
    public class CreatePaymentDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        public int? CampaignId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment type is required")]
        public PaymentType Type { get; set; }

        [MaxLength(255, ErrorMessage = "Transaction ID cannot exceed 255 characters")]
        public string? TransactionId { get; set; }

        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
}
