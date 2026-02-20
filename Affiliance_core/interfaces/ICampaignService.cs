using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.CampaignDto;
using Affiliance_core.Dto.MarkterDto;
using Affiliance_core.Entites;

namespace Affiliance_core.interfaces
{
    public interface ICampaignService
    {
        // ========================================
        // GROUP 1: Public/Anonymous Campaign Discovery
        // ========================================
        
        /// <summary>
        /// Get campaigns with filtering options
        /// </summary>
        Task<ApiResponse<PagedResult<CampaignDto>>> GetCampaignsAsync(CampaignFilterDto filter);
        
        /// <summary>
        /// Get active campaigns (status = Active and within date range)
        /// </summary>
        Task<ApiResponse<PagedResult<CampaignDto>>> GetActiveCampaignsAsync(int page = 1, int pageSize = 10);
        
        /// <summary>
        /// Get campaigns by category
        /// </summary>
        Task<ApiResponse<PagedResult<CampaignDto>>> GetCampaignsByCategoryAsync(int categoryId, int page = 1, int pageSize = 10);
        
        /// <summary>
        /// Get campaigns by company
        /// </summary>
        Task<ApiResponse<PagedResult<CampaignDto>>> GetCampaignsByCompanyAsync(int companyId, int page = 1, int pageSize = 10);
        
        /// <summary>
        /// Search campaigns with advanced filters
        /// </summary>
        Task<ApiResponse<PagedResult<CampaignDto>>> SearchCampaignsAsync(CampaignSearchDto searchDto);
        
        /// <summary>
        /// Get campaigns by status
        /// </summary>
        Task<ApiResponse<PagedResult<CampaignDto>>> GetCampaignsByStatusAsync(CampaignStatus status, int page = 1, int pageSize = 10);


        // ========================================
        // GROUP 2: Campaign Details (Public/Authorized)
        // ========================================
        
        /// <summary>
        /// Get campaign details by ID (public access)
        /// </summary>
        Task<ApiResponse<CampaignDetailsDto>> GetCampaignByIdAsync(int id);


        // ========================================
        // GROUP 3: Marketer Operations
        // ========================================
        
        /// <summary>
        /// Apply to a campaign (Marketer only)
        /// </summary>
        Task<ApiResponse<CampaignApplicationDto>> ApplyToCampaignAsync(int campaignId, int marketerId);
        
        /// <summary>
        /// Withdraw application from campaign (Marketer only)
        /// </summary>
        Task<ApiResponse<bool>> WithdrawApplicationAsync(int applicationId, int marketerId);
        
        /// <summary>
        /// Get AI-recommended campaigns for marketer
        /// </summary>
        Task<ApiResponse<PagedResult<CampaignDto>>> GetRecommendedCampaignsAsync(int marketerId, int limit = 10);


        // ========================================
        // GROUP 4: Company Owner - Campaign CRUD
        // ========================================
        
        /// <summary>
        /// Get company's own campaigns with optional filtering
        /// </summary>
        Task<ApiResponse<PagedResult<CampaignDto>>> GetMyCampaignsAsync(int companyId, CampaignFilterDto? filter = null);
        
        /// <summary>
        /// Get company's own campaign by ID
        /// </summary>
        Task<ApiResponse<CampaignDetailsDto>> GetMyCampaignByIdAsync(int campaignId, int companyId);
        
        /// <summary>
        /// Create new campaign (Company owner only)
        /// </summary>
        Task<ApiResponse<CampaignDto>> CreateCampaignAsync(CreateCampaignDto dto, int companyId);
        
        /// <summary>
        /// Update campaign (Company owner only)
        /// Restrictions apply based on campaign status
        /// </summary>
        Task<ApiResponse<CampaignDto>> UpdateCampaignAsync(int id, UpdateCampaignDto dto, int companyId);
        
        /// <summary>
        /// Delete campaign (Company owner only)
        /// Soft delete if has activity, hard delete otherwise
        /// </summary>
        Task<ApiResponse<bool>> DeleteCampaignAsync(int id, int companyId);


        // ========================================
        // GROUP 5: Company Owner - Campaign Lifecycle
        // ========================================
        
        /// <summary>
        /// Update campaign status manually (Company owner only)
        /// </summary>
        Task<ApiResponse<bool>> UpdateCampaignStatusAsync(int id, CampaignStatus status, int companyId);
        
        /// <summary>
        /// Pause active campaign temporarily (Company owner only)
        /// </summary>
        Task<ApiResponse<bool>> PauseCampaignAsync(int id, int companyId);
        
        /// <summary>
        /// Resume paused campaign (Company owner only)
        /// </summary>
        Task<ApiResponse<bool>> ResumeCampaignAsync(int id, int companyId);


        // ========================================
        // GROUP 6: Company Owner - Application Management
        // ========================================
        
        /// <summary>
        /// Get applications for a specific campaign (Company owner only)
        /// </summary>
        Task<ApiResponse<PagedResult<CampaignApplicationDto>>> GetCampaignApplicationsAsync(
            int campaignId, 
            int companyId, 
            ApplicationStatus? status = null,
            int page = 1, 
            int pageSize = 10);
        
        /// <summary>
        /// Approve marketer application and create tracking link (Company owner only)
        /// </summary>
        Task<ApiResponse<bool>> ApproveApplicationAsync(int applicationId, int companyId, string? note = null);
        
        /// <summary>
        /// Reject marketer application (Company owner only)
        /// </summary>
        Task<ApiResponse<bool>> RejectApplicationAsync(int applicationId, int companyId, string note);


        // ========================================
        // GROUP 7: Company Owner - Statistics
        // ========================================
        
        /// <summary>
        /// Get campaign statistics with optional date range
        /// </summary>
        Task<ApiResponse<CampaignStatisticsDto>> GetCampaignStatisticsAsync(
            int campaignId, 
            int companyId,
            DateTime? from = null,
            DateTime? to = null);


        // ========================================
        // GROUP 8: Admin Only - Campaign Review
        // ========================================
        
        /// <summary>
        /// Approve pending campaign (Admin only)
        /// Sets status to Active and records admin approval
        /// </summary>
        Task<ApiResponse<CampaignDto>> ApproveCampaignAsync(int id, int adminId, string? responseNote = null);
        
        /// <summary>
        /// Reject pending campaign (Admin only)
        /// </summary>
        Task<ApiResponse<CampaignDto>> RejectCampaignAsync(int id, int adminId, string responseNote);
    }
}
