namespace Affiliance_core.Dto.AnalyticsDto
{
    public class TopPerformersDto
    {
        public List<TopMarketerDto> TopMarketers { get; set; } = new List<TopMarketerDto>();
        public List<TopCampaignDto> TopCampaigns { get; set; } = new List<TopCampaignDto>();
        public List<TopCompanyDto> TopCompanies { get; set; } = new List<TopCompanyDto>();
    }

    public class TopMarketerDto
    {
        public int MarketerId { get; set; }
        public string MarketerName { get; set; } = null!;
        public decimal TotalEarnings { get; set; }
        public int TotalConversions { get; set; }
        public decimal PerformanceScore { get; set; }
        public int Rank { get; set; }
    }

    public class TopCampaignDto
    {
        public int CampaignId { get; set; }
        public string CampaignTitle { get; set; } = null!;
        public string CompanyName { get; set; } = null!;
        public int TotalConversions { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal ConversionRate { get; set; }
        public int Rank { get; set; }
    }

    public class TopCompanyDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = null!;
        public int TotalCampaigns { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalConversions { get; set; }
        public decimal AverageRating { get; set; }
        public int Rank { get; set; }
    }
}
