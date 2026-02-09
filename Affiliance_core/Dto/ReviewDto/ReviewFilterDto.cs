namespace Affiliance_core.Dto.ReviewDto
{
    public class ReviewFilterDto
    {
        public byte? Rating { get; set; }
        public int? CampaignId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
