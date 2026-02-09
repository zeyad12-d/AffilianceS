namespace Affiliance_core.Dto.ReviewDto
{
    public class AverageRatingDto
    {
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<byte, int> RatingDistribution { get; set; } = new();
    }
}
