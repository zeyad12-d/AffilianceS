using Affiliance_core.Entites;
using System.ComponentModel.DataAnnotations;

namespace Affiliance_core.Dto.ComplaintDto
{
    public class ResolveComplaintDto
    {
        [Required(ErrorMessage = "Status is required")]
        public ComplaintStatus Status { get; set; }

        [Required(ErrorMessage = "Resolution note is required")]
        [MinLength(10, ErrorMessage = "Resolution note must be at least 10 characters")]
        [MaxLength(1000, ErrorMessage = "Resolution note cannot exceed 1000 characters")]
        public string ResolutionNote { get; set; } = null!;
    }
}
