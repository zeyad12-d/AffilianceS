using System.ComponentModel.DataAnnotations;

namespace Affiliance_core.Dto.PaymentDto
{
    public class UpdatePaymentMethodDto
    {
        [MaxLength(100, ErrorMessage = "Account holder name cannot exceed 100 characters")]
        public string? AccountHolderName { get; set; }

        public bool? SetAsDefault { get; set; }
    }
}
