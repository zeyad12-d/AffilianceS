using Affiliance_core.Entites;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Affiliance_core.Entities
{
    public class AiSuggestion
    {
        [Key]
        public int Id { get; set; }

       
        public int? MarketerId { get; set; }

        public int? CompanyId { get; set; }

      
        public int? CampaignId { get; set; }

       
        public SuggestionType Type { get; set; }

        
        [Column(TypeName = "decimal(5, 3)")]
        public decimal Score { get; set; }

    
        public string? Reason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

   

        [ForeignKey("MarketerId")]
        public virtual Marketer? Marketer { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }

        [ForeignKey("CampaignId")]
        public virtual Campaign? Campaign { get; set; }
    }

    public enum SuggestionType
    {
        CampaignForMarketer, // الـ AI بيقترح حملة على مسوق
        MarketerForCompany,  // الـ AI بيقترح مسوق على شركة
        NicheExpansion       // اقتراح بتغيير التخصص بناءً على الأداء
    }
}