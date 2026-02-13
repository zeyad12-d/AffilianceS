using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.AccountDto;
using Affiliance_core.Dto.CampanyDto;
using Microsoft.AspNetCore.Http;

namespace Affiliance_core.interfaces
{
    public interface ICampanyServices
    {
        #region Registration
        /// <summary>
        /// Register a new company (Status: Pending until Admin approval)
        /// </summary>
        Task<ApiResponse<string>> RegisterCompanyAsync(CompanyRegisterDto dto);
        #endregion

        #region Company Profile Management
        /// <summary>
        /// Get company details by ID
        /// </summary>
        Task<ApiResponse<CompanyDetailsDto>> GetCompanyByIdAsync(int companyId);

        /// <summary>
        /// Get company profile (own profile)
        /// </summary>
        Task<ApiResponse<CompanyDetailsDto>> GetMyCompanyAsync(int userId);

        /// <summary>
        /// Update company information
        /// </summary>
        Task<ApiResponse<CompanyDto>> UpdateCompanyAsync(int companyId, UpdateCompanyDto dto);

        /// <summary>
        /// Update company logo
        /// </summary>
        Task<ApiResponse<string>> UpdateCompanyLogoAsync(int companyId, IFormFile logoFile);
        #endregion

        #region Statistics & Analytics
        /// <summary>
        /// Get company statistics (campaigns, applications, earnings, etc.)
        /// </summary>
        Task<ApiResponse<CompanyStatisticsDto>> GetCompanyStatisticsAsync(int companyId, DateTime? fromDate = null, DateTime? toDate = null);
        #endregion

        #region Admin Functions
        /// <summary>
        /// Get all pending companies (for Admin approval)
        /// </summary>
        Task<ApiResponse<PagedResult<CompanyApprovalDto>>> GetPendingCompaniesAsync(int page = 1, int pageSize = 10);

        /// <summary>
        /// Get all verified companies (for Admin)
        /// </summary>
        Task<ApiResponse<PagedResult<CompanyDto>>> GetVerifiedCompaniesAsync(int page = 1, int pageSize = 10);

        /// <summary>
        /// Approve company (Admin only) - changes status to Active
        /// </summary>
        Task<ApiResponse<CompanyDto>> ApproveCompanyAsync(int companyId, string? note = null);

        /// <summary>
        /// Reject company (Admin only) - user cannot login
        /// </summary>
        Task<ApiResponse<string>> RejectCompanyAsync(int companyId, string rejectReason);

        /// <summary>
        /// Suspend company (Admin only)
        /// </summary>
        Task<ApiResponse<bool>> SuspendCompanyAsync(int companyId, string? reason = null);

        /// <summary>
        /// Reactivate suspended company (Admin only)
        /// </summary>
        Task<ApiResponse<bool>> ReactivateCompanyAsync(int companyId);

        /// <summary>
        /// Verify company documents (Admin only)
        /// </summary>
        Task<ApiResponse<bool>> VerifyCompanyAsync(int companyId);

        /// <summary>
        /// Get all companies (for Admin dashboard)
        /// </summary>
        Task<ApiResponse<PagedResult<CompanyDto>>> GetAllCompaniesAsync(CompanyFilterDto filter);
        #endregion

        #region Public Search
        /// <summary>
        /// Search companies by keyword
        /// </summary>
        Task<ApiResponse<PagedResult<CompanyDto>>> SearchCompaniesAsync(string keyword, int page = 1, int pageSize = 10);
        #endregion
    }
}
