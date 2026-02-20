using Affiliance_core.Entites;

namespace Affiliance_core.Dto.PaymentDto
{
    public class WithdrawalRequestDto
    {
        public int Id { get; set; }
        public int MarketerId { get; set; }
        public string MarketerName { get; set; } = null!;
        public decimal Amount { get; set; }
        public int PaymentMethodId { get; set; }
        public string PaymentMethodType { get; set; } = null!;
        public string PaymentMethodDetails { get; set; } = null!;
        public WithdrawalStatus Status { get; set; }
        public string StatusDisplay { get; set; } = null!;
        public DateTime RequestedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? ProcessedByName { get; set; }
        public string? RejectionReason { get; set; }
        public string? AdminNotes { get; set; }
    }
}
