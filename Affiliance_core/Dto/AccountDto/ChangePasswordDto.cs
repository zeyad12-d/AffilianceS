using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Affiliance_core.Dto.AccountDto
{
    public record ChangePasswordDto
    {
      

          public string UserId { get; set; }
        [Required(ErrorMessage = "Current password is required.")]
        [MinLength(6, ErrorMessage = "Current password must be at least 6 characters long.")]
        public string CurrentPassword { get; set; }
        [Required(ErrorMessage = "New password is required.")]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters long.")]
        public string NewPassword { get; set; }
   

    }
}
