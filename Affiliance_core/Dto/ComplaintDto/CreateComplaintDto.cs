using System.ComponentModel.DataAnnotations;

namespace Affiliance_core.Dto.ComplaintDto
{
    public class CreateComplaintDto
    {
        [Required(ErrorMessage = "Defendant ID is required")]
        public int DefendantId { get; set; }

        public int? CampaignId { get; set; }

        [Required(ErrorMessage = "Subject is required")]
        [MinLength(5, ErrorMessage = "Subject must be at least 5 characters")]
        [MaxLength(200, ErrorMessage = "Subject cannot exceed 200 characters")]
        public string Subject { get; set; } = null!;

        [Required(ErrorMessage = "Description is required")]
        [MinLength(20, ErrorMessage = "Description must be at least 20 characters")]
        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; } = null!;

        [MaxLength(1000, ErrorMessage = "Evidence cannot exceed 1000 characters")]
        public string? Evidence { get; set; }
    }
}
