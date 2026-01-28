using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Affiliance_core.Entities
{
    public class PerformanceLog
    {
        [Key]
        public int Id { get; set; }

        public int TrackingLinkId { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; } 


        public PerformanceEventType EventType { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal AmountEarned { get; set; } = 0.00m;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

      

        [ForeignKey("TrackingLinkId")]
        public virtual TrackingLink TrackingLink { get; set; }
    }

    public enum PerformanceEventType
    {
        Click,      
        Lead,       
        Conversion, 
        Impression  
    }
}