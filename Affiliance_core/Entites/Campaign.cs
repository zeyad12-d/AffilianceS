using Affiliance_core.Entites;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Affiliance_core.Entites
{
    public class Campaign
    {
        [Key]
        public int Id { get; set; }

        public int CompanyId { get; set; } 

        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Title is required"), MinLength(1), MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public CommissionType CommissionType { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal CommissionValue { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal? Budget { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }

        public CampaignStatus Status { get; set; }

       

        public string? PromotionalMaterials { get; set; }

        [MaxLength(255)]
        public string? TrackingBaseUrl { get; set; }

        public int? ApprovedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        [ForeignKey("ApprovedBy")]
        public virtual User? ApprovedByNavigation { get; set; }

        public virtual ICollection<CampaignApplication> CampaignApplications { get; set; } = new List<CampaignApplication>();
        public virtual ICollection<TrackingLink> TrackingLinks { get; set; } = new List<TrackingLink>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
        public virtual ICollection<AiSuggestion> AiSuggestions { get; set; } = new List<AiSuggestion>();
    }

    public enum CommissionType
    {
        Percentage,
        Fixed
    }

    public enum CampaignStatus
    {
        Pending, 
        Active,
        Inactive,
        Paused,
        Completed,
        Rejected
    }
}