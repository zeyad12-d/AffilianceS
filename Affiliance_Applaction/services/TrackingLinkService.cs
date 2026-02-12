using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.Shared;
using Affiliance_core.Dto.TrackingLinkDto;
using Affiliance_core.Entites;
using Affiliance_core.interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Affiliance_Applaction.services
{
    public class TrackingLinkService : ITrackingLinkService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TrackingLinkService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<PagedResult<TrackingLinkDto>>> GetTrackingLinksAsync(int marketerId, TrackingLinkFilterDto? filter = null)
        {
            filter ??= new TrackingLinkFilterDto();
            var query = _unitOfWork.Repository<TrackingLink>()
                .GetQueryable()
                .Where(t => t.Marketer.Id == marketerId);

            if (filter.CampaignId.HasValue)
                query = query.Where(t => t.CampaignId == filter.CampaignId.Value);

            if (filter.IsActive.HasValue)
                query = query.Where(t => t.IsActive == filter.IsActive.Value);

            var total = await query.CountAsync();
            var links = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Include(t => t.Campaign)
                .ToListAsync();

            var linksDto = _mapper.Map<List<TrackingLinkDto>>(links);
            var pagedResult = new PagedResult<TrackingLinkDto>(linksDto, filter.Page, filter.PageSize, total);
            return ApiResponse<PagedResult<TrackingLinkDto>>.CreateSuccess(pagedResult, "Tracking links retrieved successfully");
        }

        public async Task<ApiResponse<TrackingLinkDto>> GetTrackingLinkByIdAsync(int linkId, int marketerId)
        {
            var link = await _unitOfWork.Repository<TrackingLink>()
                .FindAsync(t => t.Id == linkId && t.Marketer.Id == marketerId, new[] { "Campaign" });

            var linkEntity = link.FirstOrDefault();
            if (linkEntity == null)
                return ApiResponse<TrackingLinkDto>.CreateFail("Tracking link not found");

            var linkDto = _mapper.Map<TrackingLinkDto>(linkEntity);
            return ApiResponse<TrackingLinkDto>.CreateSuccess(linkDto, "Tracking link retrieved successfully");
        }

        public async Task<ApiResponse<TrackingLinkStatisticsDto>> GetTrackingLinkStatisticsAsync(int linkId, int marketerId)
        {
            var link = await _unitOfWork.Repository<TrackingLink>()
                .FindAsync(t => t.Id == linkId && t.Marketer.Id == marketerId);

            var linkEntity = link.FirstOrDefault();
            if (linkEntity == null)
                return ApiResponse<TrackingLinkStatisticsDto>.CreateFail("Tracking link not found");

            var stats = new TrackingLinkStatisticsDto
            {
                TotalClicks = linkEntity.Clicks,
                TotalConversions = linkEntity.Conversions,
                TotalEarnings = linkEntity.Earnings,
                ConversionRate = linkEntity.Clicks > 0 ? ((decimal)linkEntity.Conversions / linkEntity.Clicks) * 100 : 0,
                DailyStatistics = new List<DailyStatisticsDto>()
            };

            return ApiResponse<TrackingLinkStatisticsDto>.CreateSuccess(stats, "Tracking link statistics retrieved successfully");
        }

        public async Task<ApiResponse<TrackingLinkDto>> CreateTrackingLinkAsync(int marketerId, int campaignId)
        {
            var marketer = await _unitOfWork.Repository<Marketer>().GetByIdAsync(marketerId);
            if (marketer == null)
                return ApiResponse<TrackingLinkDto>.CreateFail("Marketer not found");

            var campaign = await _unitOfWork.Repository<Campaign>().GetByIdAsync(campaignId);
            if (campaign == null)
                return ApiResponse<TrackingLinkDto>.CreateFail("Campaign not found");

            var uniqueLink = GenerateUniqueLink(marketerId, campaignId);
            var trackingLink = new TrackingLink
            {
                CampaignId = campaignId,
                MarketerId = marketerId,
                UniqueLink = uniqueLink,
                Clicks = 0,
                Conversions = 0,
                Earnings = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<TrackingLink>().AddAsync(trackingLink);
            await _unitOfWork.CompleteAsync();

            var linkDto = _mapper.Map<TrackingLinkDto>(trackingLink);
            return ApiResponse<TrackingLinkDto>.CreateSuccess(linkDto, "Tracking link created successfully");
        }

        public async Task<ApiResponse<bool>> DeactivateTrackingLinkAsync(int linkId, int marketerId)
        {
            var link = await _unitOfWork.Repository<TrackingLink>()
                .FindAsync(t => t.Id == linkId && t.Marketer.Id == marketerId);

            var linkEntity = link.FirstOrDefault();
            if (linkEntity == null)
                return ApiResponse<bool>.CreateFail("Tracking link not found");

            if (!linkEntity.IsActive)
                return ApiResponse<bool>.CreateFail("Tracking link is already inactive");

            linkEntity.IsActive = false;
            _unitOfWork.Repository<TrackingLink>().Update(linkEntity);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, "Tracking link deactivated successfully");
        }

        private string GenerateUniqueLink(int marketerId, int campaignId)
        {
            var timestamp = DateTime.UtcNow.Ticks;
            var hash = System.Security.Cryptography.MD5.HashData(
                System.Text.Encoding.UTF8.GetBytes($"{marketerId}-{campaignId}-{timestamp}"));
            return System.Convert.ToBase64String(hash).Substring(0, 12).Replace("/", "").Replace("+", "");
        }
    }
}
