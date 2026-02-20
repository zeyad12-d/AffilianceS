using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.ComplaintDto;

namespace Affiliance_core.interfaces
{
    public interface IComplaintService
    {
        // ========================================
        // Complaint Creation & Management
        // ========================================
        
        /// <summary>
        /// Create a new complaint
        /// </summary>
        Task<ApiResponse<ComplaintDetailsDto>> CreateComplaintAsync(int complainantId, CreateComplaintDto dto);
        
        /// <summary>
        /// Get complaints filed by user
        /// </summary>
        Task<ApiResponse<PagedResult<ComplaintDto>>> GetMyComplaintsAsync(int userId, ComplaintFilterDto filter);
        
        /// <summary>
        /// Get complaints filed against user
        /// </summary>
        Task<ApiResponse<PagedResult<ComplaintDto>>> GetComplaintsAgainstMeAsync(int userId, ComplaintFilterDto filter);
        
        /// <summary>
        /// Get complaint details
        /// </summary>
        Task<ApiResponse<ComplaintDetailsDto>> GetComplaintDetailsAsync(int complaintId, int userId);

        // ========================================
        // Admin Complaint Management
        // ========================================
        
        /// <summary>
        /// Get all complaints with filtering (Admin)
        /// </summary>
        Task<ApiResponse<PagedResult<ComplaintDetailsDto>>> GetAllComplaintsAsync(ComplaintFilterDto filter);
        
        /// <summary>
        /// Resolve a complaint (Admin)
        /// </summary>
        Task<ApiResponse<ComplaintDetailsDto>> ResolveComplaintAsync(int complaintId, int adminId, ResolveComplaintDto dto);
        
        /// <summary>
        /// Get complaint statistics (Admin)
        /// </summary>
        Task<ApiResponse<object>> GetComplaintStatisticsAsync();
    }
}
