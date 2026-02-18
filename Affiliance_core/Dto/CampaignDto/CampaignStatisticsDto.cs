namespace Affiliance_core.Dto.CampaignDto
{
    public class CampaignStatisticsDto
    {
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int AcceptedApplications { get; set; }
        public int RejectedApplications { get; set; }
        public int WithdrawnApplications { get; set; }
        
        public int TotalClicks { get; set; }
        public int TotalConversions { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal? RemainingBudget { get; set; }
        
        // Date Range Info
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        
        // Computed Metrics
        public decimal? ConversionRate => TotalClicks > 0 
            ? Math.Round((decimal)TotalConversions / TotalClicks * 100, 2) 
            : null;
        
        public decimal? AverageRoi => TotalSpent > 0 
            ? Math.Round((TotalEarnings - TotalSpent) / TotalSpent * 100, 2) 
            : null;
    }
}
