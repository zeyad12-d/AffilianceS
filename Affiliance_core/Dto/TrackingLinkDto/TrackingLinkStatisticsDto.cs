using Affiliance_core.Dto.Shared;

namespace Affiliance_core.Dto.TrackingLinkDto
{
    public class TrackingLinkStatisticsDto
    {
        public int TotalClicks { get; set; }
        public int TotalConversions { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal ConversionRate { get; set; }
        public List<DailyStatisticsDto> DailyStatistics { get; set; } = new();
    }
}
