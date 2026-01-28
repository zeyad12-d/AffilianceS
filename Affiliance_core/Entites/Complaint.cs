using Affiliance_core.Entites;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Affiliance_core.Entities
{
    public class Complaint
    {
        [Key]
        public int Id { get; set; }

        
        public int ComplainantId { get; set; }

       
        public int DefendantId { get; set; }

        public int? CampaignId { get; set; }

        [Required, MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        
        public string? Evidence { get; set; }

    
        public ComplaintStatus Status { get; set; } = ComplaintStatus.Open;

       
        public int? ResolvedBy { get; set; }

        public string? ResolutionNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

       

        [ForeignKey("ComplainantId")]
        public virtual User Complainant { get; set; }

        [ForeignKey("DefendantId")]
        public virtual User Defendant { get; set; }

        [ForeignKey("CampaignId")]
        public virtual Campaign? Campaign { get; set; }

        [ForeignKey("ResolvedBy")]
        public virtual User? ResolvedByNavigation { get; set; }
    }

    public enum ComplaintStatus
    {
        Open,          
        InReview,      
        Resolved,       
        Dismissed,      
        ActionTaken  
    }
}