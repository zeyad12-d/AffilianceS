namespace Affiliance_core.Dto.AnalyticsDto
{
    public class AnalyticsFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? CompanyId { get; set; }
        public int? CampaignId { get; set; }
        public int? MarketerId { get; set; }
        public int? CategoryId { get; set; }
        public string GroupBy { get; set; } = "day"; // day, week, month, year
    }
}
