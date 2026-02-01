using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Affiliance_core.Dto.CampanyDto
{
    public record CompanyRegisterDto
    {
        [Required] public string CompanyName { get; set; }
        [Required][EmailAddress] public string Email { get; set; }
        [Required] public string Password { get; set; }
        [Required] public string Address { get; set; }
        [Required] public string PhoneNumber { get; set; }
        public string Website { get; set; }
        public string TaxId { get; set; }

        
        [Required] public IFormFile CommercialRegisterFile { get; set; }
        public IFormFile? LogoFile { get; set; }
    }
}
