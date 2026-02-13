namespace Affiliance_core.Dto.CampanyDto
{
    /// <summary>
    /// CompanyDto - Basic company information
    /// </summary>
    public class CompanyDto
    {
        public int Id { get; set; }
        public string CampanyName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? Description { get; set; }
        public string? ContactEmail { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
