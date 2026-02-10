namespace Affiliance_core.Dto.MarkterDto
{
    public class MarketerProfileDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? ProfilePicture { get; set; }
        public decimal Balance { get; set; }
        public string? Bio { get; set; }
        public string? Niche { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal PerformanceScore { get; set; }
        public string? CvPath { get; set; }
        public string? NationalIdPath { get; set; }
        public string? SocialLinks { get; set; }
        public string? SkillsExtracted { get; set; }
        public int? PersonalityScore { get; set; }
        public bool PersonalityTestTaken { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
