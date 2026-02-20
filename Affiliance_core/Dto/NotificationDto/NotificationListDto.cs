namespace Affiliance_core.Dto.NotificationDto
{
    public class NotificationListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public string Type { get; set; } = null!;
        public int? RelatedId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
