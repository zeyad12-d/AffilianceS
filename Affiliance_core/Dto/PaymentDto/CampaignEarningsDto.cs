namespace Affiliance_core.Dto.PaymentDto
{
    public class CampaignEarningsDto
    {
        public int CampaignId { get; set; }
        public string CampaignTitle { get; set; } = null!;
        public decimal TotalEarnings { get; set; }
        public int TotalConversions { get; set; }
        public int TotalClicks { get; set; }
        public decimal ConversionRate { get; set; }
        public DateTime FirstEarningDate { get; set; }
        public DateTime LastEarningDate { get; set; }
    }
}
