namespace Affiliance_core.Dto.AuditDto
{
    public class ActivityHistoryDto
    {
        public string Action { get; set; } = null!;
        public string? EntityType { get; set; }
        public string? EntityName { get; set; }
        public DateTime Timestamp { get; set; }
        public string Description { get; set; } = null!;
    }
}
