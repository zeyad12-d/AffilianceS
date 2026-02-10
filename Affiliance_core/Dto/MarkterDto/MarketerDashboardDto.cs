using Affiliance_core.Dto.Shared;

namespace Affiliance_core.Dto.MarkterDto
{
    public class MarketerDashboardDto
    {
        public decimal TotalEarnings { get; set; }
        public decimal Balance { get; set; }
        public int ActiveCampaigns { get; set; }
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int AcceptedApplications { get; set; }
        public decimal PerformanceScore { get; set; }
        public decimal AverageRating { get; set; }
        public List<RecentActivityDto> RecentActivities { get; set; } = new();
    }
}
