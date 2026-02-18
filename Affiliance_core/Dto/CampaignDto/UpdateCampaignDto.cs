using Affiliance_core.Entites;
using System.ComponentModel.DataAnnotations;

namespace Affiliance_core.Dto.CampaignDto
{
    public class UpdateCampaignDto
    {
        [MinLength(10, ErrorMessage = "Title must be at least 10 characters")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string? Title { get; set; }

        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        public int? CategoryId { get; set; }

        public CommissionType? CommissionType { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Commission value must be greater than 0")]
        public decimal? CommissionValue { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Budget must be greater than 0")]
        public decimal? Budget { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(500, ErrorMessage = "Promotional materials text cannot exceed 500 characters")]
        public string? PromotionalMaterials { get; set; }

        [Url(ErrorMessage = "Invalid tracking base URL")]
        [MaxLength(255, ErrorMessage = "Tracking URL cannot exceed 255 characters")]
        public string? TrackingBaseUrl { get; set; }
    }
}
