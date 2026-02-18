namespace Affiliance_core.Dto.CampaignDto
{
    public class CompanyBasicDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = null!;
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }
        public bool IsVerified { get; set; }
    }
}
