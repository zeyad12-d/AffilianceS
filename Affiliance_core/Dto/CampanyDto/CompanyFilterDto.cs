namespace Affiliance_core.Dto.CampanyDto
{
    /// <summary>
    /// CompanyFilterDto - Filtering options for companies list
    /// </summary>
    public class CompanyFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchKeyword { get; set; }
        public bool? IsVerified { get; set; }
        public string? SortBy { get; set; } = "CreatedAt"; // CreatedAt, Name, Rating
        public bool IsDescending { get; set; } = true;
    }
}
