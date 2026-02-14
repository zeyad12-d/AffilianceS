namespace Affiliance_core.Dto.ReviewDto
{
    public class CompanyReviewSummaryDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = null!;
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }
        public List<ReviewDto> RecentReviews { get; set; } = new List<ReviewDto>();
    }
}
