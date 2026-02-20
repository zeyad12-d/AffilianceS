using Affiliance_core.Entites;

namespace Affiliance_core.Dto.PaymentDto
{
    public class PaymentMethodDto
    {
        public int Id { get; set; }
        public int MarketerId { get; set; }
        public PaymentMethodType Type { get; set; }
        public string TypeDisplay { get; set; } = null!;
        public string AccountHolderName { get; set; } = null!;
        public string? MaskedAccountInfo { get; set; } // Masked for security
        public bool IsDefault { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }
}
