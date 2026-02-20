using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.AnalyticsDto;
using Affiliance_core.Entites;
using Affiliance_core.interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Affiliance_Applaction.services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AnalyticsService(IUnitOfWork unitOfWork, IMapper _mapper)
        {
            _unitOfWork = unitOfWork;
            this._mapper = _mapper;
        }

        public async Task<ApiResponse<CompanyAnalyticsDto>> GetCompanyAnalyticsAsync(int companyId, AnalyticsFilterDto filter)
        {
            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(companyId);
            if (company == null)
                return ApiResponse<CompanyAnalyticsDto>.CreateFail("Company not found");

            var startDate = filter.StartDate ?? DateTime.UtcNow.AddMonths(-1);
            var endDate = filter.EndDate ?? DateTime.UtcNow;

            var campaigns = await _unitOfWork.Repository<Campaign>()
                .GetQueryable()
                .Where(c => c.CompanyId == companyId)
                .ToListAsync();

            var activeCampaigns = campaigns.Count(c => c.Status == CampaignStatus.Active);

            var campaignIds = campaigns.Select(c => c.Id).ToList();

            var trackingLinks = await _unitOfWork.Repository<TrackingLink>()
                .GetQueryable()
                .Where(tl => campaignIds.Contains(tl.CampaignId))
                .Include(tl => tl.PerformanceLogs)
                .ToListAsync();

            var totalSpent = trackingLinks.Sum(tl => tl.Earnings);
            var totalConversions = trackingLinks.Sum(tl => tl.Conversions);
            var totalClicks = trackingLinks.Sum(tl => tl.Clicks);

            var uniqueMarketers = trackingLinks.Select(tl => tl.MarketerId).Distinct().Count();
            var activeMarketers = trackingLinks.Where(tl => tl.IsActive).Select(tl => tl.MarketerId).Distinct().Count();

            var reviews = await _unitOfWork.Repository<Review>()
                .GetQueryable()
                .Join(_unitOfWork.Repository<User>().GetQueryable(),
                      r => r.ReviewedId,
                      u => u.Id,
                      (r, u) => new { Review = r, User = u })
                .Join(_unitOfWork.Repository<Company>().GetQueryable(),
                      ru => ru.User.Id,
                      c => c.UserId,
                      (ru, c) => new { ru.Review, Company = c })
                .Where(x => x.Company.Id == companyId)
                .Select(x => x.Review)
                .ToListAsync();

            var analytics = new CompanyAnalyticsDto
            {
                CompanyId = companyId,
                CompanyName = company.CampanyName,
                TotalCampaigns = campaigns.Count,
                ActiveCampaigns = activeCampaigns,
                TotalMarketers = uniqueMarketers,
                ActiveMarketers = activeMarketers,
                TotalSpent = totalSpent,
                TotalConversions = totalConversions,
                TotalClicks = totalClicks,
                AverageConversionRate = totalClicks > 0 ? (decimal)totalConversions / totalClicks * 100 : 0,
                AverageRating = reviews.Any() ? (decimal)reviews.Average(r => (double)r.Rating) : 0,
                TotalReviews = reviews.Count,
                PeriodStart = startDate,
                PeriodEnd = endDate
            };

            return ApiResponse<CompanyAnalyticsDto>.CreateSuccess(analytics, "Company analytics retrieved successfully");
        }

        public async Task<ApiResponse<List<MarketerPerformanceDto>>> GetMarketerPerformanceAsync(int companyId, AnalyticsFilterDto filter)
        {
            var campaigns = await _unitOfWork.Repository<Campaign>()
                .GetQueryable()
                .Where(c => c.CompanyId == companyId)
                .Select(c => c.Id)
                .ToListAsync();

            var trackingLinks = await _unitOfWork.Repository<TrackingLink>()
                .GetQueryable()
                .Where(tl => campaigns.Contains(tl.CampaignId))
                .Include(tl => tl.Marketer).ThenInclude(m => m.User)
                .GroupBy(tl => tl.Marketer)
                .Select(g => new MarketerPerformanceDto
                {
                    MarketerId = g.Key.Id,
                    MarketerName = g.Key.User.FirstName + " " + g.Key.User.LastName,
                    TotalEarnings = g.Sum(tl => tl.Earnings),
                    TotalConversions = g.Sum(tl => tl.Conversions),
                    TotalClicks = g.Sum(tl => tl.Clicks),
                    ConversionRate = g.Sum(tl => tl.Clicks) > 0 ? (decimal)g.Sum(tl => tl.Conversions) / g.Sum(tl => tl.Clicks) * 100 : 0,
                    PerformanceScore = g.Key.PerformanceScore,
                    CampaignsParticipated = g.Select(tl => tl.CampaignId).Distinct().Count()
                })
                .OrderByDescending(m => m.TotalEarnings)
                .ToListAsync();

            return ApiResponse<List<MarketerPerformanceDto>>.CreateSuccess(trackingLinks, "Marketer performance retrieved successfully");
        }

        public async Task<ApiResponse<ConversionFunnelDto>> GetConversionFunnelAsync(int campaignId, AnalyticsFilterDto filter)
        {
            var campaign = await _unitOfWork.Repository<Campaign>().GetByIdAsync(campaignId);
            if (campaign == null)
                return ApiResponse<ConversionFunnelDto>.CreateFail("Campaign not found");

            var logs = await _unitOfWork.Repository<PerformanceLog>()
                .GetQueryable()
                .Join(_unitOfWork.Repository<TrackingLink>().GetQueryable(),
                      pl => pl.TrackingLinkId,
                      tl => tl.Id,
                      (pl, tl) => new { Log = pl, Link = tl })
                .Where(x => x.Link.CampaignId == campaignId)
                .Select(x => x.Log)
                .ToListAsync();

            var impressions = logs.Count(l => l.EventType == PerformanceEventType.Impression);
            var clicks = logs.Count(l => l.EventType == PerformanceEventType.Click);
            var leads = logs.Count(l => l.EventType == PerformanceEventType.Lead);
            var conversions = logs.Count(l => l.EventType == PerformanceEventType.Conversion);

            var funnel = new ConversionFunnelDto
            {
                CampaignId = campaignId,
                CampaignTitle = campaign.Title,
                TotalImpressions = impressions,
                TotalClicks = clicks,
                TotalLeads = leads,
                TotalConversions = conversions,
                ImpressionToClickRate = impressions > 0 ? (decimal)clicks / impressions * 100 : 0,
                ClickToLeadRate = clicks > 0 ? (decimal)leads / clicks * 100 : 0,
                LeadToConversionRate = leads > 0 ? (decimal)conversions / leads * 100 : 0,
                OverallConversionRate = impressions > 0 ? (decimal)conversions / impressions * 100 : 0,
                PeriodStart = filter.StartDate ?? DateTime.UtcNow.AddMonths(-1),
                PeriodEnd = filter.EndDate ?? DateTime.UtcNow
            };

            return ApiResponse<ConversionFunnelDto>.CreateSuccess(funnel, "Conversion funnel retrieved successfully");
        }

        public async Task<ApiResponse<byte[]>> ExportCampaignReportAsync(int campaignId, string format = "csv")
        {
            // Simplified - return empty byte array for now
            return ApiResponse<byte[]>.CreateSuccess(new byte[0], "Export feature not yet implemented");
        }

        public async Task<ApiResponse<PlatformOverviewDto>> GetPlatformOverviewAsync()
        {
            var totalUsers = await _unitOfWork.Repository<User>().GetQueryable().CountAsync();
            var totalMarketers = await _unitOfWork.Repository<Marketer>().GetQueryable().CountAsync();
            var totalCompanies = await _unitOfWork.Repository<Company>().GetQueryable().CountAsync();
            var totalAdmins = await _unitOfWork.Repository<Admin>().GetQueryable().CountAsync();
            var totalCampaigns = await _unitOfWork.Repository<Campaign>().GetQueryable().CountAsync();
            var activeCampaigns = await _unitOfWork.Repository<Campaign>().GetQueryable().CountAsync(c => c.Status == CampaignStatus.Active);
            
            var totalCommissions = await _unitOfWork.Repository<Payment>()
                .GetQueryable()
                .Where(p => p.Type == PaymentType.Commission && p.Status == PaymentStatus.Completed)
                .SumAsync(p => p.Amount);

            var totalConversions = await _unitOfWork.Repository<TrackingLink>().GetQueryable().SumAsync(tl => tl.Conversions);

            var pendingWithdrawals = await _unitOfWork.Repository<WithdrawalRequest>()
                .GetQueryable()
                .CountAsync(w => w.Status == WithdrawalStatus.Pending);

            var pendingAmount = await _unitOfWork.Repository<WithdrawalRequest>()
                .GetQueryable()
                .Where(w => w.Status == WithdrawalStatus.Pending)
                .SumAsync(w => w.Amount);

            var totalReviews = await _unitOfWork.Repository<Review>().GetQueryable().CountAsync();
            var totalComplaints = await _unitOfWork.Repository<Complaint>().GetQueryable().CountAsync();
            var openComplaints = await _unitOfWork.Repository<Complaint>().GetQueryable().CountAsync(c => c.Status == ComplaintStatus.Open);

            var overview = new PlatformOverviewDto
            {
                TotalUsers = totalUsers,
                TotalMarketers = totalMarketers,
                TotalCompanies = totalCompanies,
                TotalAdmins = totalAdmins,
                TotalCampaigns = totalCampaigns,
                ActiveCampaigns = activeCampaigns,
                TotalRevenue = totalCommissions,
                TotalCommissionsPaid = totalCommissions,
                TotalConversions = totalConversions,
                PendingWithdrawals = pendingWithdrawals,
                PendingWithdrawalAmount = pendingAmount,
                TotalReviews = totalReviews,
                TotalComplaints = totalComplaints,
                OpenComplaints = openComplaints,
                LastUpdated = DateTime.UtcNow
            };

            return ApiResponse<PlatformOverviewDto>.CreateSuccess(overview, "Platform overview retrieved successfully");
        }

        public async Task<ApiResponse<List<RevenueBreakdownDto>>> GetRevenueBreakdownAsync(AnalyticsFilterDto filter)
        {
            // Simplified implementation
            var breakdown = new List<RevenueBreakdownDto>();
            return ApiResponse<List<RevenueBreakdownDto>>.CreateSuccess(breakdown, "Revenue breakdown retrieved successfully");
        }

        public async Task<ApiResponse<TopPerformersDto>> GetTopPerformersAsync(int topCount = 10)
        {
            var topMarketers = await _unitOfWork.Repository<Marketer>()
                .GetQueryable()
                .Include(m => m.User)
                .OrderByDescending(m => m.TotalEarnings)
                .Take(topCount)
                .Select(m => new TopMarketerDto
                {
                    MarketerId = m.Id,
                    MarketerName = m.User.FirstName + " " + m.User.LastName,
                    TotalEarnings = m.TotalEarnings,
                    PerformanceScore = m.PerformanceScore,
                    Rank = 0 // Will be set after
                })
                .ToListAsync();

            for (int i = 0; i < topMarketers.Count; i++)
                topMarketers[i].Rank = i + 1;

            var result = new TopPerformersDto
            {
                TopMarketers = topMarketers,
                TopCampaigns = new List<TopCampaignDto>(),
                TopCompanies = new List<TopCompanyDto>()
            };

            return ApiResponse<TopPerformersDto>.CreateSuccess(result, "Top performers retrieved successfully");
        }

        public async Task<ApiResponse<byte[]>> ExportSystemReportAsync(AnalyticsFilterDto filter, string format = "csv")
        {
            return ApiResponse<byte[]>.CreateSuccess(new byte[0], "Export feature not yet implemented");
        }
    }
}
