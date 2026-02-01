using System.ComponentModel.DataAnnotations;

namespace Affiliance_core.Dto.MarkterDto
{
    public record LoginMarkterDto
    {
        [Required(ErrorMessage ="Email Is Required")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password Is Required")]

        public string Password { get; set; }
    }
}
