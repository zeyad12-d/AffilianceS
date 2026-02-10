namespace Affiliance_core.Dto.MarkterDto
{
    public class MarketerFilterDto
    {
        public string? Niche { get; set; }
        public bool? IsVerified { get; set; }
        public decimal? MinPerformanceScore { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
