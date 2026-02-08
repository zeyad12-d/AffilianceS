using System.ComponentModel.DataAnnotations;

namespace Affiliance_core.Dto.AccountDto
{
    public record LoginDto
    {
        [Required(ErrorMessage ="Email Is Required")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password Is Required")]

        public string Password { get; set; }
    }
}
