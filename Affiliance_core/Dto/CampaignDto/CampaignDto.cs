using Affiliance_core.Entites;

namespace Affiliance_core.Dto.CampaignDto
{
    public class CampaignDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public CommissionType CommissionType { get; set; }
        public decimal CommissionValue { get; set; }
        public decimal? Budget { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public CampaignStatus Status { get; set; }
        public string? PromotionalMaterials { get; set; }
        public string? TrackingBaseUrl { get; set; }
        public string? ResponseNote { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Related Data
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = null!;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        
        // Computed Properties
        public int ApplicationsCount { get; set; }
        public int AcceptedApplicationsCount { get; set; }
        public bool IsActive => Status == CampaignStatus.Active && 
                                 DateTime.UtcNow >= StartDate && 
                                 DateTime.UtcNow <= EndDate;
        public int? DaysRemaining
        {
            get
            {
                if (EndDate < DateTime.UtcNow) return null;
                return (int)(EndDate - DateTime.UtcNow).TotalDays;
            }
        }
        
        // Admin Info
        public string? ApprovedByName { get; set; }
    }
}
