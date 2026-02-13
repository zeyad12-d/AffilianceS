namespace Affiliance_core.Dto.CampanyDto
{
    /// <summary>
    /// CompanyDetailsDto - Detailed company information with statistics
    /// </summary>
    public class CompanyDetailsDto
    {
        public int Id { get; set; }
        public string CampanyName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string? TaxId { get; set; }
        public string? LogoUrl { get; set; }
        public string? Description { get; set; }
        public string? ContactEmail { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Statistics
        public int ActiveCampaignsCount { get; set; }
        public int TotalCampaignsCount { get; set; }
        public int TotalMarketerApplicationsCount { get; set; }
        public int ApprovedApplicationsCount { get; set; }
        public decimal AverageConversionRate { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal AverageRating { get; set; }
        
        // User Info
        public string? UserEmail { get; set; }
        public string? UserFirstName { get; set; }
        public string? UserLastName { get; set; }
    }
}
