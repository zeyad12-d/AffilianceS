using System.ComponentModel.DataAnnotations;

namespace Affiliance_core.Dto.CampanyDto
{
    /// <summary>
    /// UpdateCompanyDto - For updating company information
    /// </summary>
    public class UpdateCompanyDto
    {
        [MinLength(5, ErrorMessage = "Company name must be at least 5 characters")]
        public string? CampanyName { get; set; }

        [MinLength(10, ErrorMessage = "Address must be at least 10 characters")]
        public string? Address { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        public string? PhoneNumber { get; set; }

        [Url(ErrorMessage = "Invalid website URL")]
        public string? Website { get; set; }

        public string? Description { get; set; }

        [EmailAddress(ErrorMessage = "Invalid contact email")]
        public string? ContactEmail { get; set; }
    }
}
