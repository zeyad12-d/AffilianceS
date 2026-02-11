namespace Affiliance_core.Dto.Shared
{
    public class DailyStatisticsDto
    {
        public DateTime Date { get; set; }
        public int Clicks { get; set; }
        public int Conversions { get; set; }
        public decimal Earnings { get; set; }
    }
}
