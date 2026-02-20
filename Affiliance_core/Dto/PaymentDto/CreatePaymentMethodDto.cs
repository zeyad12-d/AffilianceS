using Affiliance_core.Entites;
using System.ComponentModel.DataAnnotations;

namespace Affiliance_core.Dto.PaymentDto
{
    public class CreatePaymentMethodDto
    {
        [Required(ErrorMessage = "Payment method type is required")]
        public PaymentMethodType Type { get; set; }

        [Required(ErrorMessage = "Account details are required")]
        [MaxLength(1000, ErrorMessage = "Account details cannot exceed 1000 characters")]
        public string AccountDetails { get; set; } = null!; // JSON string

        [MaxLength(100, ErrorMessage = "Account holder name cannot exceed 100 characters")]
        public string? AccountHolderName { get; set; }

        public bool SetAsDefault { get; set; } = false;
    }
}
