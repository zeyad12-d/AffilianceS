namespace Affiliance_core.Dto.ReviewDto
{
    public class MarketerPublicReviewDto
    {
        public int Id { get; set; }
        public string ReviewerName { get; set; } = null!;
        public string? ReviewerAvatar { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = null!;
        public string? CampaignTitle { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
