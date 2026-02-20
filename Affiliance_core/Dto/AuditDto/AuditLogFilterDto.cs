namespace Affiliance_core.Dto.AuditDto
{
    public class AuditLogFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? UserId { get; set; }
        public string? Action { get; set; }
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? IpAddress { get; set; }
    }
}
