using System.ComponentModel.DataAnnotations;

namespace Affiliance_core.Dto.ReviewDto
{
    public class CreateReviewDto
    {
        [Required(ErrorMessage = "Reviewed user ID is required")]
        public int ReviewedId { get; set; }

        public int? CampaignId { get; set; }

        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Comment is required")]
        [MinLength(10, ErrorMessage = "Comment must be at least 10 characters")]
        [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string Comment { get; set; } = null!;
    }
}
