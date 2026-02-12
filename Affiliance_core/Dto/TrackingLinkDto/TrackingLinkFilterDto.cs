namespace Affiliance_core.Dto.TrackingLinkDto
{
    public class TrackingLinkFilterDto
    {
        public int? CampaignId { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
