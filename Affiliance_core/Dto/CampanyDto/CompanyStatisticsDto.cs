 namespace Affiliance_core.Dto.CampanyDto
{
    /// <summary>
    /// CompanyStatisticsDto - Company performance statistics
    /// </summary>
    public class CompanyStatisticsDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        
        // Campaign Stats
        public int ActiveCampaignsCount { get; set; }
        public int PausedCampaignsCount { get; set; }
        public int CompletedCampaignsCount { get; set; }
        public int RejectedCampaignsCount { get; set; }
        
        // Application Stats
        public int TotalApplicationsCount { get; set; }
        public int ApprovedApplicationsCount { get; set; }
        public int PendingApplicationsCount { get; set; }
        public int RejectedApplicationsCount { get; set; }
        public decimal ApprovalRate { get; set; }
        
        // Performance Metrics
        public int ActiveMarketersCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCommissionPaid { get; set; }
        public decimal AverageConversionRate { get; set; }
        public decimal AverageROI { get; set; }
        public decimal AverageRating { get; set; }
        
        // Time-based Stats
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public long TotalClicks { get; set; }
        public long TotalConversions { get; set; }
    }
}
