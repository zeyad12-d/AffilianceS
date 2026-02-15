namespace Affiliance_core.Dto.AnalyticsDto
{
    public class CompanyAnalyticsDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = null!;
        public int TotalCampaigns { get; set; }
        public int ActiveCampaigns { get; set; }
        public int TotalMarketers { get; set; }
        public int ActiveMarketers { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalConversions { get; set; }
        public int TotalClicks { get; set; }
        public decimal AverageConversionRate { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
}
