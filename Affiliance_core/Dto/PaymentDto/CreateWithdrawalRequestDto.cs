using System.ComponentModel.DataAnnotations;

namespace Affiliance_core.Dto.PaymentDto
{
    public class CreateWithdrawalRequestDto
    {
        [Required(ErrorMessage = "Amount is required")]
        [Range(10, double.MaxValue, ErrorMessage = "Minimum withdrawal amount is 10")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        public int PaymentMethodId { get; set; }

        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
}
