using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.AnalyticsDto;

namespace Affiliance_core.interfaces
{
    public interface IAnalyticsService
    {
        // ========================================
        // Company Analytics
        // ========================================
        
        /// <summary>
        /// Get company analytics overview
        /// </summary>
        Task<ApiResponse<CompanyAnalyticsDto>> GetCompanyAnalyticsAsync(int companyId, AnalyticsFilterDto filter);
        
        /// <summary>
        /// Get marketer performance for company's campaigns
        /// </summary>
        Task<ApiResponse<List<MarketerPerformanceDto>>> GetMarketerPerformanceAsync(int companyId, AnalyticsFilterDto filter);
        
        /// <summary>
        /// Get conversion funnel for campaign
        /// </summary>
        Task<ApiResponse<ConversionFunnelDto>> GetConversionFunnelAsync(int campaignId, AnalyticsFilterDto filter);
        
        /// <summary>
        /// Export campaign report
        /// </summary>
        Task<ApiResponse<byte[]>> ExportCampaignReportAsync(int campaignId, string format = "csv");

        // ========================================
        // Admin Analytics
        // ========================================
        
        /// <summary>
        /// Get platform-wide overview statistics
        /// </summary>
        Task<ApiResponse<PlatformOverviewDto>> GetPlatformOverviewAsync();
        
        /// <summary>
        /// Get revenue breakdown by period
        /// </summary>
        Task<ApiResponse<List<RevenueBreakdownDto>>> GetRevenueBreakdownAsync(AnalyticsFilterDto filter);
        
        /// <summary>
        /// Get top performers (marketers, campaigns, companies)
        /// </summary>
        Task<ApiResponse<TopPerformersDto>> GetTopPerformersAsync(int topCount = 10);
        
        /// <summary>
        /// Export system report
        /// </summary>
        Task<ApiResponse<byte[]>> ExportSystemReportAsync(AnalyticsFilterDto filter, string format = "csv");
    }
}
