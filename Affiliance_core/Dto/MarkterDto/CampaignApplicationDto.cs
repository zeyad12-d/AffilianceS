using Affiliance_core.Entites;

namespace Affiliance_core.Dto.MarkterDto
{
    public class CampaignApplicationDto
    {
        public int Id { get; set; }
        public int MarketerId { get; set; }
        public int CampaignId { get; set; }
        public string CampaignTitle { get; set; }
        public ApplicationStatus Status { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string RejectReason { get; set; }
        public string MarketerName { get; set; }
        public string MarketerNiche { get; set; }
        public decimal? MarketerPerformanceScore { get; set; }
    }
}
