using System.ComponentModel.DataAnnotations;

namespace Affiliance_core.Dto.CampaignDto
{
    public class CampaignApplicationActionDto
    {
        [Required(ErrorMessage = "Application ID is required")]
        public int ApplicationId { get; set; }

        [MaxLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
        public string? Note { get; set; }
    }
}
