namespace Affiliance_core.Dto.TrackingLinkDto
{
    public class TrackingLinkDto
    {
        public int Id { get; set; }
        public int CampaignId { get; set; }
        public string CampaignTitle { get; set; }
        public string UniqueLink { get; set; }
        public int Clicks { get; set; }
        public int Conversions { get; set; }
        public decimal Earnings { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
