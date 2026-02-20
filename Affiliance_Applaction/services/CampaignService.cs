using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.CampaignDto;
using Affiliance_core.Dto.MarkterDto;
using Affiliance_core.Entites;
using Affiliance_core.interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Affiliance_Applaction.services
{
    public class CampaignService : ICampaignService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAiService _aiService;

        public CampaignService(IUnitOfWork unitOfWork, IMapper mapper, IAiService aiService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _aiService = aiService;
        }

        #region GROUP 1: Public/Anonymous Campaign Discovery

        public async Task<ApiResponse<PagedResult<CampaignDto>>> GetCampaignsAsync(CampaignFilterDto filter)
        {
            try
            {
                var query = _unitOfWork.Repository<Campaign>()
                    .GetQueryable()
                    .AsNoTracking()
                    .Include(c => c.Company)
                    .Include(c => c.Category)
                    .Include(c => c.CampaignApplications)
                    .Include(c => c.ApprovedByNavigation)
                    .AsQueryable();

                // Apply filters
                if (filter.Status.HasValue)
                    query = query.Where(c => c.Status == filter.Status.Value);

                if (filter.CategoryId.HasValue)
                    query = query.Where(c => c.CategoryId == filter.CategoryId.Value);

                if (filter.CompanyId.HasValue)
                    query = query.Where(c => c.CompanyId == filter.CompanyId.Value);

                if (filter.StartDateFrom.HasValue)
                    query = query.Where(c => c.StartDate >= filter.StartDateFrom.Value);

                if (filter.StartDateTo.HasValue)
                    query = query.Where(c => c.StartDate <= filter.StartDateTo.Value);

                if (filter.EndDateFrom.HasValue)
                    query = query.Where(c => c.EndDate >= filter.EndDateFrom.Value);

                if (filter.EndDateTo.HasValue)
                    query = query.Where(c => c.EndDate <= filter.EndDateTo.Value);

                var totalCount = await query.CountAsync();

                var campaigns = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var campaignsDto = _mapper.Map<List<CampaignDto>>(campaigns);
                var pagedResult = new PagedResult<CampaignDto>(campaignsDto, filter.Page, filter.PageSize, totalCount);

                return ApiResponse<PagedResult<CampaignDto>>.CreateSuccess(pagedResult, "Campaigns retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResult<CampaignDto>>.CreateFail($"Error retrieving campaigns: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PagedResult<CampaignDto>>> GetActiveCampaignsAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var now = DateTime.UtcNow;
                var query = _unitOfWork.Repository<Campaign>()
                    .GetQueryable()
                    .AsNoTracking()
                    .Where(c => c.Status == CampaignStatus.Active && c.StartDate <= now && c.EndDate >= now)
                    .Include(c => c.Company)
                    .Include(c => c.Category)
                    .Include(c => c.CampaignApplications)
                    .Include(c => c.ApprovedByNavigation);

                var totalCount = await query.CountAsync();

                var campaigns = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var campaignsDto = _mapper.Map<List<CampaignDto>>(campaigns);
                var pagedResult = new PagedResult<CampaignDto>(campaignsDto, page, pageSize, totalCount);

                return ApiResponse<PagedResult<CampaignDto>>.CreateSuccess(pagedResult, "Active campaigns retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResult<CampaignDto>>.CreateFail($"Error retrieving active campaigns: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PagedResult<CampaignDto>>> GetCampaignsByCategoryAsync(int categoryId, int page = 1, int pageSize = 10)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(categoryId);
            if (category == null)
                return ApiResponse<PagedResult<CampaignDto>>.CreateFail("Category not found");

            var query = _unitOfWork.Repository<Campaign>()
                .GetQueryable()
                .AsNoTracking()
                .Where(c => c.CategoryId == categoryId && c.Status == CampaignStatus.Active)
                .Include(c => c.Company)
                .Include(c => c.Category)
                .Include(c => c.CampaignApplications)
                .Include(c => c.ApprovedByNavigation);

            var totalCount = await query.CountAsync();

            var campaigns = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var campaignsDto = _mapper.Map<List<CampaignDto>>(campaigns);
            var pagedResult = new PagedResult<CampaignDto>(campaignsDto, page, pageSize, totalCount);

            return ApiResponse<PagedResult<CampaignDto>>.CreateSuccess(pagedResult, "Campaigns retrieved successfully");
        }

        public async Task<ApiResponse<PagedResult<CampaignDto>>> GetCampaignsByCompanyAsync(int companyId, int page = 1, int pageSize = 10)
        {
            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(companyId);
            if (company == null)
                return ApiResponse<PagedResult<CampaignDto>>.CreateFail("Company not found");

            var query = _unitOfWork.Repository<Campaign>()
                .GetQueryable()
                .AsNoTracking()
                .Where(c => c.CompanyId == companyId && c.Status == CampaignStatus.Active)
                .Include(c => c.Company)
                .Include(c => c.Category)
                .Include(c => c.CampaignApplications)
                .Include(c => c.ApprovedByNavigation);

            var totalCount = await query.CountAsync();

            var campaigns = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var campaignsDto = _mapper.Map<List<CampaignDto>>(campaigns);
            var pagedResult = new PagedResult<CampaignDto>(campaignsDto, page, pageSize, totalCount);

            return ApiResponse<PagedResult<CampaignDto>>.CreateSuccess(pagedResult, "Company campaigns retrieved successfully");
        }

        public async Task<ApiResponse<PagedResult<CampaignDto>>> SearchCampaignsAsync(CampaignSearchDto searchDto)
        {
            var query = _unitOfWork.Repository<Campaign>()
                .GetQueryable()
                .AsNoTracking()
                .Include(c => c.Company)
                .Include(c => c.Category)
                .Include(c => c.CampaignApplications)
                .Include(c => c.ApprovedByNavigation)
                .AsQueryable();

            // Keyword search
            if (!string.IsNullOrWhiteSpace(searchDto.Keyword))
            {
                var keyword = searchDto.Keyword.ToLower();
                query = query.Where(c => c.Title.ToLower().Contains(keyword) || 
                                        (c.Description != null && c.Description.ToLower().Contains(keyword)));
            }

            // Category filter
            if (searchDto.CategoryId.HasValue)
                query = query.Where(c => c.CategoryId == searchDto.CategoryId.Value);

            // Commission filters
            if (searchDto.MinCommission.HasValue)
                query = query.Where(c => c.CommissionValue >= searchDto.MinCommission.Value);

            if (searchDto.CommissionType.HasValue)
                query = query.Where(c => c.CommissionType == searchDto.CommissionType.Value);

            // Date filters
            if (searchDto.StartDateFrom.HasValue)
                query = query.Where(c => c.StartDate >= searchDto.StartDateFrom.Value);

            if (searchDto.StartDateTo.HasValue)
                query = query.Where(c => c.StartDate <= searchDto.StartDateTo.Value);

            if (searchDto.EndDateFrom.HasValue)
                query = query.Where(c => c.EndDate >= searchDto.EndDateFrom.Value);

            if (searchDto.EndDateTo.HasValue)
                query = query.Where(c => c.EndDate <= searchDto.EndDateTo.Value);

            // IsActive filter
            if (searchDto.IsActive.HasValue && searchDto.IsActive.Value)
            {
                var now = DateTime.UtcNow;
                query = query.Where(c => c.Status == CampaignStatus.Active && c.StartDate <= now && c.EndDate >= now);
            }

            var totalCount = await query.CountAsync();

            var campaigns = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .ToListAsync();

            var campaignsDto = _mapper.Map<List<CampaignDto>>(campaigns);
            var pagedResult = new PagedResult<CampaignDto>(campaignsDto, searchDto.Page, searchDto.PageSize, totalCount);

            return ApiResponse<PagedResult<CampaignDto>>.CreateSuccess(pagedResult, "Search completed successfully");
        }

        public async Task<ApiResponse<PagedResult<CampaignDto>>> GetCampaignsByStatusAsync(CampaignStatus status, int page = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Repository<Campaign>()
                .GetQueryable()
                .AsNoTracking()
                .Where(c => c.Status == status)
                .Include(c => c.Company)
                .Include(c => c.Category)
                .Include(c => c.CampaignApplications)
                .Include(c => c.ApprovedByNavigation);

            var totalCount = await query.CountAsync();

            var campaigns = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var campaignsDto = _mapper.Map<List<CampaignDto>>(campaigns);
            var pagedResult = new PagedResult<CampaignDto>(campaignsDto, page, pageSize, totalCount);

            return ApiResponse<PagedResult<CampaignDto>>.CreateSuccess(pagedResult, "Campaigns retrieved successfully");
        }

        #endregion

        #region GROUP 2: Campaign Details (Public/Authorized)

        public async Task<ApiResponse<CampaignDetailsDto>> GetCampaignByIdAsync(int id)
        {
            try
            {
                var campaign = await _unitOfWork.Repository<Campaign>()
                    .GetQueryable()
                    .AsNoTracking()
                    .Where(c => c.Id == id)
                    .Include(c => c.Company)
                    .Include(c => c.Category)
                    .Include(c => c.CampaignApplications)
                    .Include(c => c.ApprovedByNavigation)
                    .FirstOrDefaultAsync();

                if (campaign == null)
                    return ApiResponse<CampaignDetailsDto>.CreateFail("Campaign not found");

                var campaignDto = _mapper.Map<CampaignDetailsDto>(campaign);
                return ApiResponse<CampaignDetailsDto>.CreateSuccess(campaignDto, "Campaign retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CampaignDetailsDto>.CreateFail($"Error retrieving campaign: {ex.Message}");
            }
        }

        #endregion

        #region GROUP 3: Marketer Operations

        public async Task<ApiResponse<CampaignApplicationDto>> ApplyToCampaignAsync(int campaignId, int marketerId)
        {
            // Verify campaign exists and is active
            var campaign = await _unitOfWork.Repository<Campaign>().GetByIdAsync(campaignId);
            if (campaign == null)
                return ApiResponse<CampaignApplicationDto>.CreateFail("Campaign not found");

            if (campaign.Status != CampaignStatus.Active)
                return ApiResponse<CampaignApplicationDto>.CreateFail("Campaign is not active");

            var now = DateTime.UtcNow;
            if (campaign.StartDate > now || campaign.EndDate < now)
                return ApiResponse<CampaignApplicationDto>.CreateFail("Campaign is not within active date range");

            // Verify marketer exists
            var marketer = await _unitOfWork.Repository<Marketer>().GetByIdAsync(marketerId);
            if (marketer == null)
                return ApiResponse<CampaignApplicationDto>.CreateFail("Marketer not found");

            // Check if already applied
            var existingApplication = await _unitOfWork.Repository<CampaignApplication>()
                .GetQueryable()
                .FirstOrDefaultAsync(a => a.CampaignId == campaignId && a.MarketerId == marketerId);

            if (existingApplication != null)
                return ApiResponse<CampaignApplicationDto>.CreateFail("You have already applied to this campaign");

            // Create new application
            var application = new CampaignApplication
            {
                CampaignId = campaignId,
                MarketerId = marketerId,
                Status = ApplicationStatus.Pending,
                AppliedAt = DateTime.UtcNow,
                AiMatchScore = null // Will be calculated by AI service if needed
            };

            await _unitOfWork.Repository<CampaignApplication>().AddAsync(application);
            await _unitOfWork.CompleteAsync();

            // Reload with navigation properties for DTO mapping
            application = await _unitOfWork.Repository<CampaignApplication>()
                .GetQueryable()
                .Include(a => a.Campaign)
                .Include(a => a.Marketer).ThenInclude(m => m.User)
                .FirstOrDefaultAsync(a => a.Id == application.Id);

            var applicationDto = _mapper.Map<CampaignApplicationDto>(application);
            return ApiResponse<CampaignApplicationDto>.CreateSuccess(applicationDto, "Application submitted successfully");
        }

        public async Task<ApiResponse<bool>> WithdrawApplicationAsync(int applicationId, int marketerId)
        {
            var application = await _unitOfWork.Repository<CampaignApplication>()
                .GetQueryable()
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.MarketerId == marketerId);

            if (application == null)
                return ApiResponse<bool>.CreateFail("Application not found");

            if (application.Status != ApplicationStatus.Pending)
                return ApiResponse<bool>.CreateFail("Can only withdraw pending applications");

            application.Status = ApplicationStatus.Withdrawn;
            application.RespondedAt = DateTime.UtcNow;
            application.ResponseNote = "Withdrawn by marketer";

            _unitOfWork.Repository<CampaignApplication>().Update(application);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, "Application withdrawn successfully");
        }

        public async Task<ApiResponse<PagedResult<CampaignDto>>> GetRecommendedCampaignsAsync(int marketerId, int limit = 10)
        {
            // Verify marketer exists
            var marketer = await _unitOfWork.Repository<Marketer>().GetByIdAsync(marketerId);
            if (marketer == null)
                return ApiResponse<PagedResult<CampaignDto>>.CreateFail("Marketer not found");

            // Get AI suggestions for this marketer
            var suggestions = await _unitOfWork.Repository<AiSuggestion>()
                .GetQueryable()
                .Where(s => s.MarketerId == marketerId && s.CampaignId.HasValue)
                .OrderByDescending(s => s.Score)
                .Take(limit)
                .Select(s => s.CampaignId!.Value)
                .ToListAsync();

            if (!suggestions.Any())
            {
                // Fallback: Get active campaigns
                var fallbackQuery = _unitOfWork.Repository<Campaign>()
                    .GetQueryable()
                    .Where(c => c.Status == CampaignStatus.Active)
                    .Include(c => c.Company)
                    .Include(c => c.Category)
                    .Include(c => c.CampaignApplications)
                    .Include(c => c.ApprovedByNavigation)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(limit);

                var fallbackCampaigns = await fallbackQuery.ToListAsync();
                var fallbackDto = _mapper.Map<List<CampaignDto>>(fallbackCampaigns);
                var fallbackResult = new PagedResult<CampaignDto>(fallbackDto, 1, limit, fallbackDto.Count);
                return ApiResponse<PagedResult<CampaignDto>>.CreateSuccess(fallbackResult, "Recommended campaigns retrieved successfully");
            }

            var campaigns = await _unitOfWork.Repository<Campaign>()
                .GetQueryable()
                .Where(c => suggestions.Contains(c.Id))
                .Include(c => c.Company)
                .Include(c => c.Category)
                .Include(c => c.CampaignApplications)
                .Include(c => c.ApprovedByNavigation)
                .ToListAsync();

            // Preserve order from suggestions
            var orderedCampaigns = suggestions
                .Select(id => campaigns.FirstOrDefault(c => c.Id == id))
                .Where(c => c != null)
                .ToList();

            var campaignsDto = _mapper.Map<List<CampaignDto>>(orderedCampaigns);
            var pagedResult = new PagedResult<CampaignDto>(campaignsDto, 1, limit, campaignsDto.Count);

            return ApiResponse<PagedResult<CampaignDto>>.CreateSuccess(pagedResult, "Recommended campaigns retrieved successfully");
        }

        #endregion

        #region GROUP 4: Company Owner - Campaign CRUD

        public async Task<ApiResponse<PagedResult<CampaignDto>>> GetMyCampaignsAsync(int companyId, CampaignFilterDto? filter = null)
        {
            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(companyId);
            if (company == null)
                return ApiResponse<PagedResult<CampaignDto>>.CreateFail("Company not found");

            filter ??= new CampaignFilterDto();

            var query = _unitOfWork.Repository<Campaign>()
                .GetQueryable()
                .Where(c => c.CompanyId == companyId)
                .Include(c => c.Company)
                .Include(c => c.Category)
                .Include(c => c.CampaignApplications)
                .Include(c => c.ApprovedByNavigation)
                .AsQueryable();

            // Apply filters (same as GetCampaignsAsync)
            if (filter.Status.HasValue)
                query = query.Where(c => c.Status == filter.Status.Value);

            if (filter.CategoryId.HasValue)
                query = query.Where(c => c.CategoryId == filter.CategoryId.Value);

            if (filter.StartDateFrom.HasValue)
                query = query.Where(c => c.StartDate >= filter.StartDateFrom.Value);

            if (filter.StartDateTo.HasValue)
                query = query.Where(c => c.StartDate <= filter.StartDateTo.Value);

            if (filter.EndDateFrom.HasValue)
                query = query.Where(c => c.EndDate >= filter.EndDateFrom.Value);

            if (filter.EndDateTo.HasValue)
                query = query.Where(c => c.EndDate <= filter.EndDateTo.Value);

            var totalCount = await query.CountAsync();

            var campaigns = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var campaignsDto = _mapper.Map<List<CampaignDto>>(campaigns);
            var pagedResult = new PagedResult<CampaignDto>(campaignsDto, filter.Page, filter.PageSize, totalCount);

            return ApiResponse<PagedResult<CampaignDto>>.CreateSuccess(pagedResult, "Your campaigns retrieved successfully");
        }

        public async Task<ApiResponse<CampaignDetailsDto>> GetMyCampaignByIdAsync(int campaignId, int companyId)
        {
            var campaign = await _unitOfWork.Repository<Campaign>()
                .GetQueryable()
                .Where(c => c.Id == campaignId && c.CompanyId == companyId)
                .Include(c => c.Company)
                .Include(c => c.Category)
                .Include(c => c.CampaignApplications)
                .Include(c => c.ApprovedByNavigation)
                .FirstOrDefaultAsync();

            if (campaign == null)
                return ApiResponse<CampaignDetailsDto>.CreateFail("Campaign not found or access denied");

            var campaignDto = _mapper.Map<CampaignDetailsDto>(campaign);

            // Get statistics
            var statsResponse = await GetCampaignStatisticsAsync(campaignId, companyId);
            if (statsResponse.Success)
                campaignDto.Statistics = statsResponse.Data;

            return ApiResponse<CampaignDetailsDto>.CreateSuccess(campaignDto, "Campaign retrieved successfully");
        }

        public async Task<ApiResponse<CampaignDto>> CreateCampaignAsync(CreateCampaignDto dto, int companyId)
        {
            // Verify company exists and is verified
            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(companyId);
            if (company == null)
                return ApiResponse<CampaignDto>.CreateFail("Company not found");

            if (!company.IsVerified)
                return ApiResponse<CampaignDto>.CreateFail("Company must be verified to create campaigns");

            // Verify category exists
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(dto.CategoryId);
            if (category == null)
                return ApiResponse<CampaignDto>.CreateFail("Category not found");

            // Validate dates
            if (dto.EndDate <= dto.StartDate)
                return ApiResponse<CampaignDto>.CreateFail("End date must be after start date");

            if (dto.StartDate < DateTime.UtcNow.Date)
                return ApiResponse<CampaignDto>.CreateFail("Start date cannot be in the past");

            // Create campaign
            var campaign = _mapper.Map<Campaign>(dto);
            campaign.CompanyId = companyId;
            campaign.Status = CampaignStatus.Pending; // Requires admin approval
            campaign.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Campaign>().AddAsync(campaign);
            await _unitOfWork.CompleteAsync();

            // Reload with navigation properties
            campaign = await _unitOfWork.Repository<Campaign>()
                .GetQueryable()
                .Where(c => c.Id == campaign.Id)
                .Include(c => c.Company)
                .Include(c => c.Category)
                .Include(c => c.CampaignApplications)
                .Include(c => c.ApprovedByNavigation)
                .FirstOrDefaultAsync();

            var campaignDto = _mapper.Map<CampaignDto>(campaign);
            return ApiResponse<CampaignDto>.CreateSuccess(campaignDto, "Campaign created successfully and pending admin approval");
        }

        public async Task<ApiResponse<CampaignDto>> UpdateCampaignAsync(int id, UpdateCampaignDto dto, int companyId)
        {
            var campaign = await _unitOfWork.Repository<Campaign>()
                .GetQueryable()
                .Where(c => c.Id == id && c.CompanyId == companyId)
                .Include(c => c.Company)
                .Include(c => c.Category)
                .Include(c => c.CampaignApplications)
                .Include(c => c.ApprovedByNavigation)
                .FirstOrDefaultAsync();

            if (campaign == null)
                return ApiResponse<CampaignDto>.CreateFail("Campaign not found or access denied");

            // Check if campaign can be edited based on status
            if (campaign.Status == CampaignStatus.Completed)
                return ApiResponse<CampaignDto>.CreateFail("Cannot edit completed campaigns");

            if (campaign.Status == CampaignStatus.Rejected)
                return ApiResponse<CampaignDto>.CreateFail("Cannot edit rejected campaigns");

            // If active, only allow limited updates
            if (campaign.Status == CampaignStatus.Active)
            {
                if (dto.Title != null || dto.StartDate.HasValue || dto.EndDate.HasValue || 
                    dto.CategoryId.HasValue || dto.CommissionType.HasValue || dto.CommissionValue.HasValue)
                {
                    return ApiResponse<CampaignDto>.CreateFail("Cannot modify core fields of active campaigns. Only Description and PromotionalMaterials can be updated.");
                }
            }

            // Validate category if changed
            if (dto.CategoryId.HasValue)
            {
                var category = await _unitOfWork.Repository<Category>().GetByIdAsync(dto.CategoryId.Value);
                if (category == null)
                    return ApiResponse<CampaignDto>.CreateFail("Category not found");
            }

            // Validate dates if changed
            var newStartDate = dto.StartDate ?? campaign.StartDate;
            var newEndDate = dto.EndDate ?? campaign.EndDate;
            
            if (newEndDate <= newStartDate)
                return ApiResponse<CampaignDto>.CreateFail("End date must be after start date");

            // Apply updates
            _mapper.Map(dto, campaign);

            _unitOfWork.Repository<Campaign>().Update(campaign);
            await _unitOfWork.CompleteAsync();

            var campaignDto = _mapper.Map<CampaignDto>(campaign);
            return ApiResponse<CampaignDto>.CreateSuccess(campaignDto, "Campaign updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteCampaignAsync(int id, int companyId)
        {
            var campaign = await _unitOfWork.Repository<Campaign>()
                .GetQueryable()
                .Where(c => c.Id == id && c.CompanyId == companyId)
                .Include(c => c.CampaignApplications)
                .Include(c => c.TrackingLinks)
                .FirstOrDefaultAsync();

            if (campaign == null)
                return ApiResponse<bool>.CreateFail("Campaign not found or access denied");

            // Cannot delete active campaigns
            if (campaign.Status == CampaignStatus.Active)
                return ApiResponse<bool>.CreateFail("Cannot delete active campaigns. Please pause or complete it first.");

            // Check if has any activity
            var hasApplications = campaign.CampaignApplications?.Any() ?? false;
            var hasTrackingLinks = campaign.TrackingLinks?.Any() ?? false;

            if (hasApplications || hasTrackingLinks)
            {
                // Soft delete: Set status to Inactive
                campaign.Status = CampaignStatus.Inactive;
                _unitOfWork.Repository<Campaign>().Update(campaign);
                await _unitOfWork.CompleteAsync();
                return ApiResponse<bool>.CreateSuccess(true, "Campaign marked as inactive (soft delete) due to existing activity");
            }
            else
            {
                // Hard delete: No activity
                _unitOfWork.Repository<Campaign>().Delete(campaign);
                await _unitOfWork.CompleteAsync();
                return ApiResponse<bool>.CreateSuccess(true, "Campaign deleted successfully");
            }
        }

        #endregion

        #region GROUP 5: Company Owner - Campaign Lifecycle

        public async Task<ApiResponse<bool>> UpdateCampaignStatusAsync(int id, CampaignStatus status, int companyId)
        {
            var campaign = await _unitOfWork.Repository<Campaign>()
                .GetQueryable()
                .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId);

            if (campaign == null)
                return ApiResponse<bool>.CreateFail("Campaign not found or access denied");

            // Validate status transitions
            if (status == CampaignStatus.Pending || status == CampaignStatus.Rejected)
                return ApiResponse<bool>.CreateFail("Only admin can set campaign to Pending or Rejected status");

            campaign.Status = status;
            _unitOfWork.Repository<Campaign>().Update(campaign);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, $"Campaign status updated to {status}");
        }

        public async Task<ApiResponse<bool>> PauseCampaignAsync(int id, int companyId)
        {
            var campaign = await _unitOfWork.Repository<Campaign>()
                .GetQueryable()
                .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId);

            if (campaign == null)
                return ApiResponse<bool>.CreateFail("Campaign not found or access denied");

            if (campaign.Status != CampaignStatus.Active)
                return ApiResponse<bool>.CreateFail("Only active campaigns can be paused");

            campaign.Status = CampaignStatus.Paused;
            _unitOfWork.Repository<Campaign>().Update(campaign);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, "Campaign paused successfully");
        }

        public async Task<ApiResponse<bool>> ResumeCampaignAsync(int id, int companyId)
        {
            var campaign = await _unitOfWork.Repository<Campaign>()
                .GetQueryable()
                .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId);

            if (campaign == null)
                return ApiResponse<bool>.CreateFail("Campaign not found or access denied");

            if (campaign.Status != CampaignStatus.Paused)
                return ApiResponse<bool>.CreateFail("Only paused campaigns can be resumed");

            // Check if still within date range
            var now = DateTime.UtcNow;
            if (campaign.EndDate < now)
                return ApiResponse<bool>.CreateFail("Cannot resume campaign that has ended");

            campaign.Status = CampaignStatus.Active;
            _unitOfWork.Repository<Campaign>().Update(campaign);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, "Campaign resumed successfully");
        }

        #endregion

        #region GROUP 6: Company Owner - Application Management

        public async Task<ApiResponse<PagedResult<CampaignApplicationDto>>> GetCampaignApplicationsAsync(
            int campaignId, 
            int companyId, 
            ApplicationStatus? status = null, 
            int page = 1, 
            int pageSize = 10)
        {
            // Verify campaign ownership
            var campaign = await _unitOfWork.Repository<Campaign>()
                .GetQueryable()
                .FirstOrDefaultAsync(c => c.Id == campaignId && c.CompanyId == companyId);

            if (campaign == null)
                return ApiResponse<PagedResult<CampaignApplicationDto>>.CreateFail("Campaign not found or access denied");

            var query = _unitOfWork.Repository<CampaignApplication>()
                .GetQueryable()
                .Where(a => a.CampaignId == campaignId)
                .Include(a => a.Campaign)
                .Include(a => a.Marketer).ThenInclude(m => m.User)
                .AsQueryable();

            // Filter by status if provided
            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            var totalCount = await query.CountAsync();

            var applications = await query
                .OrderByDescending(a => a.AiMatchScore ?? 0)
                .ThenByDescending(a => a.AppliedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var applicationsDto = _mapper.Map<List<CampaignApplicationDto>>(applications);
            var pagedResult = new PagedResult<CampaignApplicationDto>(applicationsDto, page, pageSize, totalCount);

            return ApiResponse<PagedResult<CampaignApplicationDto>>.CreateSuccess(pagedResult, "Applications retrieved successfully");
        }

        public async Task<ApiResponse<bool>> ApproveApplicationAsync(int applicationId, int companyId, string? note = null)
        {
            var application = await _unitOfWork.Repository<CampaignApplication>()
                .GetQueryable()
                .Include(a => a.Campaign)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
                return ApiResponse<bool>.CreateFail("Application not found");

            // Verify campaign ownership
            if (application.Campaign.CompanyId != companyId)
                return ApiResponse<bool>.CreateFail("Access denied");

            if (application.Status != ApplicationStatus.Pending)
                return ApiResponse<bool>.CreateFail("Only pending applications can be approved");

            // Update application
            application.Status = ApplicationStatus.Accepted;
            application.RespondedAt = DateTime.UtcNow;
            application.ResponseNote = note;

            _unitOfWork.Repository<CampaignApplication>().Update(application);

            // Create tracking link for the marketer
            var existingTrackingLink = await _unitOfWork.Repository<TrackingLink>()
                .GetQueryable()
                .FirstOrDefaultAsync(t => t.CampaignId == application.CampaignId && t.MarketerId == application.MarketerId);

            if (existingTrackingLink == null)
            {
                var trackingLink = new TrackingLink
                {
                    CampaignId = application.CampaignId,
                    MarketerId = application.MarketerId,
                    UniqueLink = GenerateUniqueTrackingLink(application.CampaignId, application.MarketerId),
                    Clicks = 0,
                    Conversions = 0,
                    Earnings = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<TrackingLink>().AddAsync(trackingLink);
            }

            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, "Application approved and tracking link created successfully");
        }

        public async Task<ApiResponse<bool>> RejectApplicationAsync(int applicationId, int companyId, string note)
        {
            if (string.IsNullOrWhiteSpace(note))
                return ApiResponse<bool>.CreateFail("Rejection note is required");

            var application = await _unitOfWork.Repository<CampaignApplication>()
                .GetQueryable()
                .Include(a => a.Campaign)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
                return ApiResponse<bool>.CreateFail("Application not found");

            // Verify campaign ownership
            if (application.Campaign.CompanyId != companyId)
                return ApiResponse<bool>.CreateFail("Access denied");

            if (application.Status != ApplicationStatus.Pending)
                return ApiResponse<bool>.CreateFail("Only pending applications can be rejected");

            // Update application
            application.Status = ApplicationStatus.Rejected;
            application.RespondedAt = DateTime.UtcNow;
            application.ResponseNote = note;

            _unitOfWork.Repository<CampaignApplication>().Update(application);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, "Application rejected successfully");
        }

        #endregion

        #region GROUP 7: Company Owner - Statistics

        public async Task<ApiResponse<CampaignStatisticsDto>> GetCampaignStatisticsAsync(
            int campaignId, 
            int companyId, 
            DateTime? from = null, 
            DateTime? to = null)
        {
            // Verify campaign ownership
            var campaign = await _unitOfWork.Repository<Campaign>()
                .GetQueryable()
                .FirstOrDefaultAsync(c => c.Id == campaignId && c.CompanyId == companyId);

            if (campaign == null)
                return ApiResponse<CampaignStatisticsDto>.CreateFail("Campaign not found or access denied");

            // Get applications statistics
            var applicationsQuery = _unitOfWork.Repository<CampaignApplication>()
                .GetQueryable()
                .Where(a => a.CampaignId == campaignId);

            var totalApplications = await applicationsQuery.CountAsync();
            var pendingApplications = await applicationsQuery.CountAsync(a => a.Status == ApplicationStatus.Pending);
            var acceptedApplications = await applicationsQuery.CountAsync(a => a.Status == ApplicationStatus.Accepted);
            var rejectedApplications = await applicationsQuery.CountAsync(a => a.Status == ApplicationStatus.Rejected);
            var withdrawnApplications = await applicationsQuery.CountAsync(a => a.Status == ApplicationStatus.Withdrawn);

            // Get tracking statistics
            var trackingQuery = _unitOfWork.Repository<TrackingLink>()
                .GetQueryable()
                .Where(t => t.CampaignId == campaignId);

            // Apply date filters if provided (would need CreatedAt on TrackingLink clicks/conversions)
            // For now, we get all-time stats

            var totalClicks = await trackingQuery.SumAsync(t => t.Clicks);
            var totalConversions = await trackingQuery.SumAsync(t => t.Conversions);
            var totalEarnings = await trackingQuery.SumAsync(t => (decimal?)t.Earnings) ?? 0;

            // Calculate total spent (could be based on conversions * commission)
            decimal totalSpent = 0;
            if (campaign.CommissionType == CommissionType.Fixed)
            {
                totalSpent = totalConversions * campaign.CommissionValue;
            }
            else // Percentage
            {
                totalSpent = totalEarnings; // Earnings already calculated with percentage
            }

            // Calculate remaining budget
            decimal? remainingBudget = null;
            if (campaign.Budget.HasValue)
            {
                remainingBudget = campaign.Budget.Value - totalSpent;
            }

            var statistics = new CampaignStatisticsDto
            {
                TotalApplications = totalApplications,
                PendingApplications = pendingApplications,
                AcceptedApplications = acceptedApplications,
                RejectedApplications = rejectedApplications,
                WithdrawnApplications = withdrawnApplications,
                TotalClicks = totalClicks,
                TotalConversions = totalConversions,
                TotalEarnings = totalEarnings,
                TotalSpent = totalSpent,
                RemainingBudget = remainingBudget,
                DateFrom = from,
                DateTo = to
            };

            return ApiResponse<CampaignStatisticsDto>.CreateSuccess(statistics, "Statistics retrieved successfully");
        }

        #endregion

        #region GROUP 8: Admin Only - Campaign Review

        public async Task<ApiResponse<CampaignDto>> ApproveCampaignAsync(int id, int adminId, string? responseNote = null)
        {
            var campaign = await _unitOfWork.Repository<Campaign>()
                .GetQueryable()
                .Where(c => c.Id == id)
                .Include(c => c.Company)
                .Include(c => c.Category)
                .Include(c => c.CampaignApplications)
                .Include(c => c.ApprovedByNavigation)
                .FirstOrDefaultAsync();

            if (campaign == null)
                return ApiResponse<CampaignDto>.CreateFail("Campaign not found");

            if (campaign.Status != CampaignStatus.Pending)
                return ApiResponse<CampaignDto>.CreateFail("Only pending campaigns can be approved");

            // Verify admin exists (check via Admin entity or role claim)
            var admin = await _unitOfWork.Repository<User>().GetByIdAsync(adminId);
            if (admin == null)
                return ApiResponse<CampaignDto>.CreateFail("Invalid admin user");

            // Approve campaign
            campaign.Status = CampaignStatus.Active;
            campaign.ApprovedBy = adminId;

            _unitOfWork.Repository<Campaign>().Update(campaign);
            await _unitOfWork.CompleteAsync();

            var campaignDto = _mapper.Map<CampaignDto>(campaign);
            return ApiResponse<CampaignDto>.CreateSuccess(campaignDto, "Campaign approved successfully");
        }

        public async Task<ApiResponse<CampaignDto>> RejectCampaignAsync(int id, int adminId, string responseNote)
        {
            if (string.IsNullOrWhiteSpace(responseNote))
                return ApiResponse<CampaignDto>.CreateFail("Rejection note is required");

            var campaign = await _unitOfWork.Repository<Campaign>()
                .GetQueryable()
                .Where(c => c.Id == id)
                .Include(c => c.Company)
                .Include(c => c.Category)
                .Include(c => c.CampaignApplications)
                .Include(c => c.ApprovedByNavigation)
                .FirstOrDefaultAsync();

            if (campaign == null)
                return ApiResponse<CampaignDto>.CreateFail("Campaign not found");

            if (campaign.Status != CampaignStatus.Pending)
                return ApiResponse<CampaignDto>.CreateFail("Only pending campaigns can be rejected");

            // Verify admin exists (check via Admin entity or role claim)
            var admin = await _unitOfWork.Repository<User>().GetByIdAsync(adminId);
            if (admin == null)
                return ApiResponse<CampaignDto>.CreateFail("Invalid admin user");

            // Reject campaign
            campaign.Status = CampaignStatus.Rejected;
            campaign.ApprovedBy = adminId;

            _unitOfWork.Repository<Campaign>().Update(campaign);
            await _unitOfWork.CompleteAsync();

            var campaignDto = _mapper.Map<CampaignDto>(campaign);
            return ApiResponse<CampaignDto>.CreateSuccess(campaignDto, "Campaign rejected");
        }

        #endregion

        #region Helper Methods

        private string GenerateUniqueTrackingLink(int campaignId, int marketerId)
        {
            // Generate a unique tracking code
            var uniqueCode = Guid.NewGuid().ToString("N").Substring(0, 12);
            return $"https://track.affiliance.com/c{campaignId}m{marketerId}/{uniqueCode}";
        }

        #endregion
    }
}

