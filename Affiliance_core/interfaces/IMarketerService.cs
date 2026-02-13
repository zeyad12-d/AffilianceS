using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.MarkterDto;
using Affiliance_core.Entites;
using Microsoft.AspNetCore.Http;

namespace Affiliance_core.interfaces
{
    public interface IMarketerService
    {
        Task<ApiResponse<MarketerProfileDto>> GetMyProfileAsync(int marketerId);
        Task<ApiResponse<MarketerPublicDto>> GetMarketerByIdAsync(int id);
        Task<ApiResponse<PagedResult<MarketerPublicDto>>> GetMarketersAsync(MarketerFilterDto filter);
        Task<ApiResponse<PagedResult<MarketerPublicDto>>> SearchMarketersAsync(MarketerSearchDto searchDto);
        Task<ApiResponse<PagedResult<MarketerPublicDto>>> GetMarketersByNicheAsync(string niche, int page = 1, int pageSize = 10);
        Task<ApiResponse<PagedResult<MarketerPublicDto>>> GetRecommendedMarketersAsync(int campaignId, int limit = 10);
        
        Task<ApiResponse<MarketerProfileDto>> UpdateProfileAsync(int marketerId, UpdateMarketerProfileDto dto);
        Task<ApiResponse<string>> UpdateCvAsync(int marketerId, IFormFile cvFile);
        Task<ApiResponse<string>> UpdateNationalIdAsync(int marketerId, IFormFile nationalIdFile);
        Task<ApiResponse<bool>> UpdateSkillsAsync(int marketerId, string skills);
        Task<ApiResponse<bool>> UpdateBioAsync(int marketerId, string bio);
        Task<ApiResponse<bool>> UpdateNicheAsync(int marketerId, string niche);
        Task<ApiResponse<bool>> UpdateSocialLinksAsync(int marketerId, string socialLinks);
        
        Task<ApiResponse<PersonalityTestResultDto>> SubmitPersonalityTestAsync(int marketerId, PersonalityTestDto testDto);
        
        Task<ApiResponse<PagedResult<CampaignApplicationDto>>> GetMyApplicationsAsync(int marketerId, ApplicationFilterDto? filter = null);
        Task<ApiResponse<CampaignApplicationDto>> GetApplicationByIdAsync(int applicationId, int marketerId);
        Task<ApiResponse<PagedResult<CampaignApplicationDto>>> GetApplicationsByStatusAsync(int marketerId, ApplicationStatus status, int page = 1, int pageSize = 10);
        
        Task<ApiResponse<bool>> WithdrawApplicationAsync(int applicationId, int marketerId);
        
        Task<ApiResponse<MarketerDashboardDto>> GetDashboardAsync(int marketerId);
        Task<ApiResponse<MarketerStatisticsDto>> GetStatisticsAsync(int marketerId, DateTime? startDate, DateTime? endDate);
        Task<ApiResponse<EarningsReportDto>> GetEarningsReportAsync(int marketerId, DateTime? startDate, DateTime? endDate, string? groupBy = "month");
        Task<ApiResponse<List<PerformanceHistoryDto>>> GetPerformanceHistoryAsync(int marketerId);
        
        Task<ApiResponse<PagedResult<AiSuggestionDto>>> GetAiSuggestionsAsync(int marketerId, int limit = 10);
        Task<ApiResponse<PersonalityTestResultDto>> GetPersonalityTestResultsAsync(int marketerId);
        
        Task<ApiResponse<bool>> VerifyMarketerAsync(int marketerId);
        Task<ApiResponse<bool>> UnverifyMarketerAsync(int marketerId);
        Task<ApiResponse<bool>> UpdatePerformanceScoreAsync(int marketerId, decimal performanceScore);
        Task<ApiResponse<PagedResult<MarketerPublicDto>>> GetPendingVerificationMarketersAsync(int page = 1, int pageSize = 10);
    }
}
