namespace Affiliance_core.Dto.AnalyticsDto
{
    public class RevenueBreakdownDto
    {
        public string Period { get; set; } = null!; // Year-Month or Year-Week
        public decimal TotalRevenue { get; set; }
        public decimal CommissionsPaid { get; set; }
        public decimal PlatformFees { get; set; }
        public decimal NetRevenue { get; set; }
        public int TotalTransactions { get; set; }
        public Dictionary<string, decimal> RevenueByCategory { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> RevenueByCompany { get; set; } = new Dictionary<string, decimal>();
    }
}
