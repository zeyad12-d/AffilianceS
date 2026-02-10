namespace Affiliance_core.Dto.MarkterDto
{
    public class MarketerPublicDto
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Bio { get; set; }
        public string? Niche { get; set; }
        public decimal PerformanceScore { get; set; }
        public string? SocialLinks { get; set; }
        public string? SkillsExtracted { get; set; }
        public int? PersonalityScore { get; set; }
        public bool IsVerified { get; set; }
        public decimal AverageRating { get; set; }
        public int ReviewsCount { get; set; }
    }
}
