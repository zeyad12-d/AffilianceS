namespace Affiliance_core.Dto.AnalyticsDto
{
    public class PlatformOverviewDto
    {
        public int TotalUsers { get; set; }
        public int TotalMarketers { get; set; }
        public int TotalCompanies { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalCampaigns { get; set; }
        public int ActiveCampaigns { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCommissionsPaid { get; set; }
        public int TotalConversions { get; set; }
        public int PendingWithdrawals { get; set; }
        public decimal PendingWithdrawalAmount { get; set; }
        public int TotalReviews { get; set; }
        public int TotalComplaints { get; set; }
        public int OpenComplaints { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
