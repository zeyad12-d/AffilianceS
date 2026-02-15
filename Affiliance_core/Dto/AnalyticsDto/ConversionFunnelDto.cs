namespace Affiliance_core.Dto.AnalyticsDto
{
    public class ConversionFunnelDto
    {
        public int CampaignId { get; set; }
        public string CampaignTitle { get; set; } = null!;
        public int TotalImpressions { get; set; }
        public int TotalClicks { get; set; }
        public int TotalLeads { get; set; }
        public int TotalConversions { get; set; }
        public decimal ImpressionToClickRate { get; set; }
        public decimal ClickToLeadRate { get; set; }
        public decimal LeadToConversionRate { get; set; }
        public decimal OverallConversionRate { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
}
