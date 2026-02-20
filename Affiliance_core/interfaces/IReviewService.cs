using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.ReviewDto;

namespace Affiliance_core.interfaces
{
    public interface IReviewService
    {
        Task<ApiResponse<PagedResult<ReviewDto>>> GetReviewsAsync(int reviewedId, ReviewFilterDto? filter = null);
        Task<ApiResponse<PagedResult<ReviewDto>>> GetReviewsGivenAsync(int reviewerId, int page = 1, int pageSize = 10);
        Task<ApiResponse<AverageRatingDto>> GetAverageRatingAsync(int reviewedId);
        Task<ApiResponse<ReviewDto>> CreateReviewAsync(int reviewerId, int reviewedId, int? campaignId, byte rating, string? comment);
        Task<ApiResponse<ReviewDto>> UpdateReviewAsync(int reviewId, int reviewerId, UpdateReviewDto dto);
        Task<ApiResponse<bool>> DeleteReviewAsync(int reviewId, int reviewerId);
        Task<ApiResponse<CompanyReviewSummaryDto>> GetCompanyReviewSummaryAsync(int companyId);
        Task<ApiResponse<PagedResult<MarketerPublicReviewDto>>> GetMarketerPublicReviewsAsync(int marketerId, int page = 1, int pageSize = 10);
    }
}
