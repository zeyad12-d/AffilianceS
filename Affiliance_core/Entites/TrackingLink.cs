using Affiliance_core.Entites;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Affiliance_core.Entites
{
    public class TrackingLink
    {
        [Key]
        public int Id { get; set; }

        public int CampaignId { get; set; }

        public int MarketerId { get; set; }

        [Required]
        [MaxLength(500)]
        public string UniqueLink { get; set; } 

        public int Clicks { get; set; } = 0;

        public int Conversions { get; set; } = 0;

        [Column(TypeName = "decimal(12, 2)")]
        public decimal Earnings { get; set; } = 0.00m;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        
        public bool IsActive { get; set; } = true;

       

        [ForeignKey("CampaignId")]
        public virtual Campaign Campaign { get; set; }

        [ForeignKey("MarketerId")]
        public virtual Marketer Marketer { get; set; }

        public virtual ICollection<PerformanceLog> PerformanceLogs { get; set; } = new List<PerformanceLog>();
    }
}