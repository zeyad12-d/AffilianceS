namespace Affiliance_core.Dto.MarkterDto
{
    public class MarketerStatisticsDto
    {
        public int TotalApplications { get; set; }
        public int AcceptedApplications { get; set; }
        public int RejectedApplications { get; set; }
        public int TotalClicks { get; set; }
        public int TotalConversions { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal AverageEarningsPerConversion { get; set; }
        public decimal ConversionRate { get; set; }
    }
}
