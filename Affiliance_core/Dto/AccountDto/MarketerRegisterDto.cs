using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Affiliance_core.Dto.AccountDto
{
    public class MarketerRegisterDto
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public IFormFile NationalIdImage { get; set; }
    }
}
