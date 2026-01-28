using Affiliance_core.Entites;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Affiliance_core.Entities
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

      
        public int ReviewerId { get; set; }

       
        public int ReviewedId { get; set; }

       
        public int? CampaignId { get; set; }

        [Range(1, 5)] 
        public byte Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        

        [ForeignKey("ReviewerId")]
        public virtual User Reviewer { get; set; }

        [ForeignKey("ReviewedId")]
        public virtual User Reviewed { get; set; }

        [ForeignKey("CampaignId")]
        public virtual Campaign? Campaign { get; set; }
    }
}