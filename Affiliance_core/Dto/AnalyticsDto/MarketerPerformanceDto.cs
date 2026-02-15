namespace Affiliance_core.Dto.AnalyticsDto
{
    public class MarketerPerformanceDto
    {
        public int MarketerId { get; set; }
        public string MarketerName { get; set; } = null!;
        public string? MarketerAvatar { get; set; }
        public decimal TotalEarnings { get; set; }
        public int TotalConversions { get; set; }
        public int TotalClicks { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal PerformanceScore { get; set; }
        public int CampaignsParticipated { get; set; }
        public DateTime? FirstConversionDate { get; set; }
        public DateTime? LastConversionDate { get; set; }
    }
}
