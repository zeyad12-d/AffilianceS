namespace Affiliance_core.Dto.MarkterDto
{
    public class EarningsReportDto
    {
        public decimal TotalEarnings { get; set; }
        public List<EarningsByPeriodDto> EarningsByPeriod { get; set; } = new();
    }

    public class EarningsByPeriodDto
    {
        public string? Period { get; set; }
        public decimal Earnings { get; set; }
        public int Conversions { get; set; }
    }
}
