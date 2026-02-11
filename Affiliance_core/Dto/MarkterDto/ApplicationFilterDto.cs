using Affiliance_core.Entites;

namespace Affiliance_core.Dto.MarkterDto
{
    public class ApplicationFilterDto
    {
        public ApplicationStatus? Status { get; set; }
        public int? CampaignId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
