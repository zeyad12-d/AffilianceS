using Affiliance_core.Entites;

namespace Affiliance_core.Dto.ComplaintDto
{
    public class ComplaintDetailsDto
    {
        public int Id { get; set; }
        public int ComplainantId { get; set; }
        public string ComplainantName { get; set; } = null!;
        public string? ComplainantEmail { get; set; }
        public int DefendantId { get; set; }
        public string DefendantName { get; set; } = null!;
        public string? DefendantEmail { get; set; }
        public int? CampaignId { get; set; }
        public string? CampaignTitle { get; set; }
        public string Subject { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Evidence { get; set; }
        public ComplaintStatus Status { get; set; }
        public string StatusDisplay { get; set; } = null!;
        public int? ResolvedBy { get; set; }
        public string? ResolvedByName { get; set; }
        public string? ResolutionNote { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}
