namespace Affiliance_core.Dto.MarkterDto
{
    public class MarketerSearchDto
    {
        public string? Keyword { get; set; }
        public string? Niche { get; set; }
        public string? Skills { get; set; }
        public bool? IsVerified { get; set; }
        public decimal? MinPerformanceScore { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
