using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.TrackingLinkDto;

namespace Affiliance_core.interfaces
{
    public interface ITrackingLinkService
    {
        Task<ApiResponse<PagedResult<TrackingLinkDto>>> GetTrackingLinksAsync(int marketerId, TrackingLinkFilterDto? filter = null);
        Task<ApiResponse<TrackingLinkDto>> GetTrackingLinkByIdAsync(int linkId, int marketerId);
        Task<ApiResponse<TrackingLinkStatisticsDto>> GetTrackingLinkStatisticsAsync(int linkId, int marketerId);
        Task<ApiResponse<TrackingLinkDto>> CreateTrackingLinkAsync(int marketerId, int campaignId);
        Task<ApiResponse<bool>> DeactivateTrackingLinkAsync(int linkId, int marketerId);
    }
}
