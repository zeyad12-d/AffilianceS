using Affiliance_core.Entites;

namespace Affiliance_core.Dto.PaymentDto
{
    public class PaymentFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? UserId { get; set; }
        public int? CampaignId { get; set; }
        public PaymentType? Type { get; set; }
        public PaymentStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
