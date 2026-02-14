namespace Affiliance_core.Dto.MarkterDto
{
    public class PublicMarketerProfileDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string? ProfilePicture { get; set; }
        public string? Bio { get; set; }
        public string? Niche { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public decimal PerformanceScore { get; set; }
        public string? SocialLinks { get; set; }
        public bool IsVerified { get; set; }
        public DateTime MemberSince { get; set; }
        public int TotalCampaignsCompleted { get; set; }
    }
}
