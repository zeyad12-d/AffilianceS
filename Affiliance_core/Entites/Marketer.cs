using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Affiliance_core.Entites
{
    public class Marketer
    {
        [Key]
        public int Id { get; set; }


        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }


        public string Bio { get; set; }

        public string Niche { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalEarnings { get; set; } = 0.00m;

        [Range(0, 5)]
        public decimal PerformanceScore { get; set; } = 0.00m;

        public string CvPath { get; set; }

        public string SocialLinks { get; set; }

        // AI Data 
        public string SkillsExtracted { get; set; }
        public int? PersonalityScore { get; set; }
        public bool PersonalityTestTaken { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

       
    }
}
