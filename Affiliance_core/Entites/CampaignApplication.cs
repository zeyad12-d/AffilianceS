using Affiliance_core.Entites;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Affiliance_core.Entities
{
    public class CampaignApplication
    {
        [Key]
        public int Id { get; set; }

        public int CampaignId { get; set; }

        public int MarketerId { get; set; }

        
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        [Column(TypeName = "decimal(5, 3)")]
        public decimal? AiMatchScore { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RespondedAt { get; set; }

        [MaxLength(500)] 
        public string? ResponseNote { get; set; }

 

        [ForeignKey("CampaignId")]
        public virtual Campaign Campaign { get; set; }

        [ForeignKey("MarketerId")]
        public virtual Marketer Marketer { get; set; }
    }

    public enum ApplicationStatus
    {
        Pending,   
        Accepted, 
        Rejected,  
        Withdrawn 
    }
}