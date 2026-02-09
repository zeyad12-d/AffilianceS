namespace Affiliance_core.Dto.ReviewDto
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int ReviewerId { get; set; }
        public string ReviewerName { get; set; }
        public int ReviewedId { get; set; }
        public int? CampaignId { get; set; }
        public string? CampaignTitle { get; set; }
        public byte Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
