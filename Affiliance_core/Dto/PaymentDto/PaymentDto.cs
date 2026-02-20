using Affiliance_core.Entites;

namespace Affiliance_core.Dto.PaymentDto
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public int? CampaignId { get; set; }
        public string? CampaignTitle { get; set; }
        public decimal Amount { get; set; }
        public PaymentType Type { get; set; }
        public string TypeDisplay { get; set; } = null!;
        public PaymentStatus Status { get; set; }
        public string StatusDisplay { get; set; } = null!;
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
