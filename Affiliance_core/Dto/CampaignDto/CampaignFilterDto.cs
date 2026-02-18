using Affiliance_core.Entites;

namespace Affiliance_core.Dto.CampaignDto
{
    public class CampaignFilterDto
    {
        public CampaignStatus? Status { get; set; }
        public int? CategoryId { get; set; }
        public int? CompanyId { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
