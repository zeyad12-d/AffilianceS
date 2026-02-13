namespace Affiliance_core.Dto.CampanyDto
{
    /// <summary>
    /// CompanyApprovalDto - Company pending approval info for Admin
    /// </summary>
    public class CompanyApprovalDto
    {
        public int Id { get; set; }
        public string CampanyName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string? TaxId { get; set; }
        public string? Description { get; set; }
        public string CommercialRegister { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // User Info
        public string? UserEmail { get; set; }
        public string? UserFirstName { get; set; }
        public string? UserLastName { get; set; }
        
        // Status
        public string Status { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
    }
}
