using Affiliance_core.Entites;

namespace Affiliance_core.Dto.ComplaintDto
{
    public class ComplaintFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? ComplainantId { get; set; }
        public int? DefendantId { get; set; }
        public int? CampaignId { get; set; }
        public ComplaintStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SearchTerm { get; set; }
    }
}
