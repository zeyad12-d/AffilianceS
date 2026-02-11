using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.MarkterDto;
using Affiliance_core.Dto.Shared;
using Affiliance_core.Entites;
using Affiliance_core.interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Affiliance_Applaction.services
{
    public class MarketerService : IMarketerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;

        public MarketerService(IUnitOfWork unitOfWork, IMapper mapper, IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileService = fileService;
        }

        public async Task<ApiResponse<MarketerProfileDto>> GetMyProfileAsync(int marketerId)
        {
            var marketer = await _unitOfWork.Repository<Marketer>()
                .FindAsync(m => m.Id == marketerId, new[] { "User" });

            var marketerEntity = marketer.FirstOrDefault();
            if (marketerEntity == null)
                return ApiResponse<MarketerProfileDto>.CreateFail("Marketer not found");

            var marketerDto = _mapper.Map<MarketerProfileDto>(marketerEntity);
            return ApiResponse<MarketerProfileDto>.CreateSuccess(marketerDto, "Profile retrieved successfully");
        }

        public async Task<ApiResponse<MarketerPublicDto>> GetMarketerByIdAsync(int id)
        {
            var marketer = await _unitOfWork.Repository<Marketer>()
                .FindAsync(m => m.Id == id, new[] { "User" });

            var marketerEntity = marketer.FirstOrDefault();
            if (marketerEntity == null)
                return ApiResponse<MarketerPublicDto>.CreateFail("Marketer not found");

            var marketerDto = _mapper.Map<MarketerPublicDto>(marketerEntity);
            return ApiResponse<MarketerPublicDto>.CreateSuccess(marketerDto, "Marketer retrieved successfully");
        }

        public async Task<ApiResponse<PagedResult<MarketerPublicDto>>> GetMarketersAsync(MarketerFilterDto filter)
        {
            var query = _unitOfWork.Repository<Marketer>().GetQueryable();

            if (filter.IsVerified.HasValue)
                query = query.Where(m => m.IsVerified == filter.IsVerified.Value);

            if (!string.IsNullOrEmpty(filter.Niche))
                query = query.Where(m => m.Niche.Contains(filter.Niche));

            if (filter.MinPerformanceScore.HasValue)
                query = query.Where(m => m.PerformanceScore >= filter.MinPerformanceScore.Value);

            var total = await query.CountAsync();
            var marketers = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Include(m => m.User)
                .ToListAsync();

            var marketersDto = _mapper.Map<List<MarketerPublicDto>>(marketers);
            var pagedResult = new PagedResult<MarketerPublicDto>(marketersDto, filter.Page, filter.PageSize, total);
            return ApiResponse<PagedResult<MarketerPublicDto>>.CreateSuccess(pagedResult, "Marketers retrieved successfully");
        }

        public async Task<ApiResponse<PagedResult<MarketerPublicDto>>> SearchMarketersAsync(MarketerSearchDto searchDto)
        {
            var query = _unitOfWork.Repository<Marketer>().GetQueryable();

            if (!string.IsNullOrEmpty(searchDto.Keyword))
            {
                query = query.Where(m => m.User.FirstName.Contains(searchDto.Keyword) ||
                                        m.User.LastName.Contains(searchDto.Keyword) ||
                                        m.Bio.Contains(searchDto.Keyword));
            }

            if (!string.IsNullOrEmpty(searchDto.Niche))
                query = query.Where(m => m.Niche.Contains(searchDto.Niche));

            if (!string.IsNullOrEmpty(searchDto.Skills))
                query = query.Where(m => m.SkillsExtracted.Contains(searchDto.Skills));

            if (searchDto.IsVerified.HasValue)
                query = query.Where(m => m.IsVerified == searchDto.IsVerified.Value);

            if (searchDto.MinPerformanceScore.HasValue)
                query = query.Where(m => m.PerformanceScore >= searchDto.MinPerformanceScore.Value);

            var total = await query.CountAsync();
            var marketers = await query
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .Include(m => m.User)
                .ToListAsync();

            var marketersDto = _mapper.Map<List<MarketerPublicDto>>(marketers);
            var pagedResult = new PagedResult<MarketerPublicDto>(marketersDto, searchDto.Page, searchDto.PageSize, total);
            return ApiResponse<PagedResult<MarketerPublicDto>>.CreateSuccess(pagedResult, "Search completed successfully");
        }

        public async Task<ApiResponse<PagedResult<MarketerPublicDto>>> GetMarketersByNicheAsync(string niche, int page = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Repository<Marketer>()
                .GetQueryable()
                .Where(m => m.Niche.Contains(niche));

            var total = await query.CountAsync();
            var marketers = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(m => m.User)
                .ToListAsync();

            var marketersDto = _mapper.Map<List<MarketerPublicDto>>(marketers);
            var pagedResult = new PagedResult<MarketerPublicDto>(marketersDto, page, pageSize, total);
            return ApiResponse<PagedResult<MarketerPublicDto>>.CreateSuccess(pagedResult, "Marketers retrieved successfully");
        }

        public async Task<ApiResponse<PagedResult<MarketerPublicDto>>> GetRecommendedMarketersAsync(int campaignId, int limit = 10)
        {
            var campaign = await _unitOfWork.Repository<Campaign>().GetByIdAsync(campaignId);
            if (campaign == null)
                return ApiResponse<PagedResult<MarketerPublicDto>>.CreateFail("Campaign not found");

            var marketers = await _unitOfWork.Repository<Marketer>()
                .GetQueryable()
                .Where(m => m.IsVerified && m.PerformanceScore > 0)
                .OrderByDescending(m => m.PerformanceScore)
                .Take(limit)
                .Include(m => m.User)
                .ToListAsync();

            var marketersDto = _mapper.Map<List<MarketerPublicDto>>(marketers);
            var pagedResult = new PagedResult<MarketerPublicDto>(marketersDto, 1, limit, marketersDto.Count);
            return ApiResponse<PagedResult<MarketerPublicDto>>.CreateSuccess(pagedResult, "Recommended marketers retrieved successfully");
        }

        public async Task<ApiResponse<MarketerProfileDto>> UpdateProfileAsync(int marketerId, UpdateMarketerProfileDto dto)
        {
            var marketer = await _unitOfWork.Repository<Marketer>().GetByIdAsync(marketerId);
            if (marketer == null)
                return ApiResponse<MarketerProfileDto>.CreateFail("Marketer not found");

            if (!string.IsNullOrEmpty(dto.Bio)) marketer.Bio = dto.Bio;
            if (!string.IsNullOrEmpty(dto.Niche)) marketer.Niche = dto.Niche;
            if (!string.IsNullOrEmpty(dto.SocialLinks)) marketer.SocialLinks = dto.SocialLinks;
            if (!string.IsNullOrEmpty(dto.SkillsExtracted)) marketer.SkillsExtracted = dto.SkillsExtracted;

            marketer.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Marketer>().Update(marketer);
            await _unitOfWork.CompleteAsync();

            var marketerDto = _mapper.Map<MarketerProfileDto>(marketer);
            return ApiResponse<MarketerProfileDto>.CreateSuccess(marketerDto, "Profile updated successfully");
        }

        public async Task<ApiResponse<string>> UpdateCvAsync(int marketerId, IFormFile cvFile)
        {
            if (cvFile == null || cvFile.Length == 0)
                return ApiResponse<string>.CreateFail("CV file is required");

            var marketer = await _unitOfWork.Repository<Marketer>().GetByIdAsync(marketerId);
            if (marketer == null)
                return ApiResponse<string>.CreateFail("Marketer not found");

            var result = await _fileService.UploadFileAsync(cvFile, "cvs");
            // Since result is a string path, check for null/empty
            if (string.IsNullOrEmpty(result))
                return ApiResponse<string>.CreateFail("Failed to upload CV");

            marketer.CvPath = result;
            marketer.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Marketer>().Update(marketer);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<string>.CreateSuccess(result, "CV updated successfully");
        }

        public async Task<ApiResponse<string>> UpdateNationalIdAsync(int marketerId, IFormFile nationalIdFile)
        {
            if (nationalIdFile == null || nationalIdFile.Length == 0)
                return ApiResponse<string>.CreateFail("National ID file is required");

            var marketer = await _unitOfWork.Repository<Marketer>().GetByIdAsync(marketerId);
            if (marketer == null)
                return ApiResponse<string>.CreateFail("Marketer not found");

            var result = await _fileService.UploadFileAsync(nationalIdFile, "national_ids");
            // Since result is a string path, check for null/empty
            if (string.IsNullOrEmpty(result))
                return ApiResponse<string>.CreateFail("Failed to upload National ID");

            marketer.NationalIdPath = result;
            marketer.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Marketer>().Update(marketer);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<string>.CreateSuccess(result, "National ID updated successfully");
        }

        public async Task<ApiResponse<bool>> UpdateSkillsAsync(int marketerId, string skills)
        {
            var marketer = await _unitOfWork.Repository<Marketer>().GetByIdAsync(marketerId);
            if (marketer == null)
                return ApiResponse<bool>.CreateFail("Marketer not found");

            marketer.SkillsExtracted = skills;
            marketer.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Marketer>().Update(marketer);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, "Skills updated successfully");
        }

        public async Task<ApiResponse<bool>> UpdateBioAsync(int marketerId, string bio)
        {
            var marketer = await _unitOfWork.Repository<Marketer>().GetByIdAsync(marketerId);
            if (marketer == null)
                return ApiResponse<bool>.CreateFail("Marketer not found");

            marketer.Bio = bio;
            marketer.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Marketer>().Update(marketer);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, "Bio updated successfully");
        }

        public async Task<ApiResponse<bool>> UpdateNicheAsync(int marketerId, string niche)
        {
            var marketer = await _unitOfWork.Repository<Marketer>().GetByIdAsync(marketerId);
            if (marketer == null)
                return ApiResponse<bool>.CreateFail("Marketer not found");

            marketer.Niche = niche;
            marketer.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Marketer>().Update(marketer);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, "Niche updated successfully");
        }

        public async Task<ApiResponse<bool>> UpdateSocialLinksAsync(int marketerId, string socialLinks)
        {
            var marketer = await _unitOfWork.Repository<Marketer>().GetByIdAsync(marketerId);
            if (marketer == null)
                return ApiResponse<bool>.CreateFail("Marketer not found");

            marketer.SocialLinks = socialLinks;
            marketer.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Marketer>().Update(marketer);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, "Social links updated successfully");
        }

        public async Task<ApiResponse<PersonalityTestResultDto>> SubmitPersonalityTestAsync(int marketerId, PersonalityTestDto testDto)
        {
            var marketer = await _unitOfWork.Repository<Marketer>().GetByIdAsync(marketerId);
            if (marketer == null)
                return ApiResponse<PersonalityTestResultDto>.CreateFail("Marketer not found");

            var score = testDto.Answers.Sum(a => a.Answer);
            var personalityType = DeterminePersonalityType(score);

            marketer.PersonalityScore = score;
            marketer.PersonalityTestTaken = true;
            marketer.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Marketer>().Update(marketer);
            await _unitOfWork.CompleteAsync();

            var result = new PersonalityTestResultDto
            {
                PersonalityScore = score,
                PersonalityType = personalityType,
                Description = GetPersonalityDescription(personalityType),
                TestDate = DateTime.UtcNow
            };

            return ApiResponse<PersonalityTestResultDto>.CreateSuccess(result, "Personality test submitted successfully");
        }

        public async Task<ApiResponse<PagedResult<CampaignApplicationDto>>> GetMyApplicationsAsync(int marketerId, ApplicationFilterDto? filter = null)
        {
            filter ??= new ApplicationFilterDto();
            var query = _unitOfWork.Repository<CampaignApplication>()
                .GetQueryable()
                .Where(a => a.MarketerId == marketerId);

            if (filter.Status.HasValue)
                query = query.Where(a => a.Status == filter.Status.Value);

            if (filter.CampaignId.HasValue)
                query = query.Where(a => a.CampaignId == filter.CampaignId.Value);

            var total = await query.CountAsync();
            var applications = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Include(a => a.Campaign)
                .ToListAsync();

            var applicationsDto = _mapper.Map<List<CampaignApplicationDto>>(applications);
            var pagedResult = new PagedResult<CampaignApplicationDto>(applicationsDto, filter.Page, filter.PageSize, total);
            return ApiResponse<PagedResult<CampaignApplicationDto>>.CreateSuccess(pagedResult, "Applications retrieved successfully");
        }

        public async Task<ApiResponse<CampaignApplicationDto>> GetApplicationByIdAsync(int applicationId, int marketerId)
        {
            var application = await _unitOfWork.Repository<CampaignApplication>()
                .FindAsync(a => a.Id == applicationId && a.MarketerId == marketerId, new[] { "Campaign" });

            var applicationEntity = application.FirstOrDefault();
            if (applicationEntity == null)
                return ApiResponse<CampaignApplicationDto>.CreateFail("Application not found");

            var applicationDto = _mapper.Map<CampaignApplicationDto>(applicationEntity);
            return ApiResponse<CampaignApplicationDto>.CreateSuccess(applicationDto, "Application retrieved successfully");
        }

        public async Task<ApiResponse<PagedResult<CampaignApplicationDto>>> GetApplicationsByStatusAsync(int marketerId, ApplicationStatus status, int page = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Repository<CampaignApplication>()
                .GetQueryable()
                .Where(a => a.MarketerId == marketerId && a.Status == status);

            var total = await query.CountAsync();
            var applications = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(a => a.Campaign)
                .ToListAsync();

            var applicationsDto = _mapper.Map<List<CampaignApplicationDto>>(applications);
            var pagedResult = new PagedResult<CampaignApplicationDto>(applicationsDto, page, pageSize, total);
            return ApiResponse<PagedResult<CampaignApplicationDto>>.CreateSuccess(pagedResult, "Applications retrieved successfully");
        }

        public async Task<ApiResponse<bool>> WithdrawApplicationAsync(int applicationId, int marketerId)
        {
            var application = await _unitOfWork.Repository<CampaignApplication>().GetByIdAsync(applicationId);
            if (application == null)
                return ApiResponse<bool>.CreateFail("Application not found");

            if (application.MarketerId != marketerId)
                return ApiResponse<bool>.CreateFail("You can only withdraw your own applications");

            if (application.Status == ApplicationStatus.Rejected || application.Status == ApplicationStatus.Withdrawn)
                return ApiResponse<bool>.CreateFail("Cannot withdraw this application");

            application.Status = ApplicationStatus.Withdrawn;
            _unitOfWork.Repository<CampaignApplication>().Update(application);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, "Application withdrawn successfully");
        }


        public async Task<ApiResponse<MarketerDashboardDto>> GetDashboardAsync(int marketerId)
        {
            var marketer = await _unitOfWork.Repository<Marketer>().GetByIdAsync(marketerId);
            if (marketer == null)
                return ApiResponse<MarketerDashboardDto>.CreateFail("Marketer not found");

            var applications = await _unitOfWork.Repository<CampaignApplication>()
                .FindAsync(a => a.MarketerId == marketerId);

            var dashboard = new MarketerDashboardDto
            {
                TotalEarnings = marketer.TotalEarnings,
                Balance = marketer.TotalEarnings,
                ActiveCampaigns = applications.Count(a => a.Status == ApplicationStatus.Accepted),
                TotalApplications = applications.Count(),
                PendingApplications = applications.Count(a => a.Status == ApplicationStatus.Pending),
                AcceptedApplications = applications.Count(a => a.Status == ApplicationStatus.Accepted),
                PerformanceScore = marketer.PerformanceScore,
                AverageRating = 4.5m,
                RecentActivities = new List<RecentActivityDto>()
            };

            return ApiResponse<MarketerDashboardDto>.CreateSuccess(dashboard, "Dashboard retrieved successfully");
        }

        public async Task<ApiResponse<MarketerStatisticsDto>> GetStatisticsAsync(int marketerId, DateTime? startDate, DateTime? endDate)
        {
            var applications = await _unitOfWork.Repository<CampaignApplication>()
                .FindAsync(a => a.MarketerId == marketerId);

            var stats = new MarketerStatisticsDto
            {
                TotalApplications = applications.Count(),
                AcceptedApplications = applications.Count(a => a.Status == ApplicationStatus.Accepted),
                RejectedApplications = applications.Count(a => a.Status == ApplicationStatus.Rejected),
                TotalClicks = 0,
                TotalConversions = 0,
                TotalEarnings = 0,
                AverageEarningsPerConversion = 0,
                ConversionRate = 0
            };

            return ApiResponse<MarketerStatisticsDto>.CreateSuccess(stats, "Statistics retrieved successfully");
        }

        public async Task<ApiResponse<EarningsReportDto>> GetEarningsReportAsync(int marketerId, DateTime? startDate, DateTime? endDate, string? groupBy = "month")
        {
            var report = new EarningsReportDto
            {
                TotalEarnings = 0,
                EarningsByPeriod = new List<EarningsByPeriodDto>()
            };

            return ApiResponse<EarningsReportDto>.CreateSuccess(report, "Earnings report retrieved successfully");
        }

        public async Task<ApiResponse<List<PerformanceHistoryDto>>> GetPerformanceHistoryAsync(int marketerId)
        {
            var marketer = await _unitOfWork.Repository<Marketer>().GetByIdAsync(marketerId);
            if (marketer == null)
                return ApiResponse<List<PerformanceHistoryDto>>.CreateFail("Marketer not found");

            var history = new List<PerformanceHistoryDto>
            {
                new PerformanceHistoryDto
                {
                    Date = DateTime.UtcNow,
                    PerformanceScore = marketer.PerformanceScore
                }
            };

            return ApiResponse<List<PerformanceHistoryDto>>.CreateSuccess(history, "Performance history retrieved successfully");
        }


        public async Task<ApiResponse<PagedResult<AiSuggestionDto>>> GetAiSuggestionsAsync(int marketerId, int limit = 10)
        {
            var suggestions = await _unitOfWork.Repository<AiSuggestion>()
                .FindAsync(a => a.MarketerId == marketerId);

            var suggestionsDto = _mapper.Map<List<AiSuggestionDto>>(suggestions.Take(limit));
            var pagedResult = new PagedResult<AiSuggestionDto>(suggestionsDto, 1, limit, suggestionsDto.Count);
            return ApiResponse<PagedResult<AiSuggestionDto>>.CreateSuccess(pagedResult, "AI suggestions retrieved successfully");
        }

        public async Task<ApiResponse<PersonalityTestResultDto>> GetPersonalityTestResultsAsync(int marketerId)
        {
            var marketer = await _unitOfWork.Repository<Marketer>().GetByIdAsync(marketerId);
            if (marketer == null)
                return ApiResponse<PersonalityTestResultDto>.CreateFail("Marketer not found");

            if (!marketer.PersonalityTestTaken || !marketer.PersonalityScore.HasValue)
                return ApiResponse<PersonalityTestResultDto>.CreateFail("Personality test not completed");

            var result = new PersonalityTestResultDto
            {
                PersonalityScore = marketer.PersonalityScore.Value,
                PersonalityType = DeterminePersonalityType(marketer.PersonalityScore.Value),
                Description = GetPersonalityDescription(DeterminePersonalityType(marketer.PersonalityScore.Value)),
                TestDate = marketer.CreatedAt
            };

            return ApiResponse<PersonalityTestResultDto>.CreateSuccess(result, "Personality test results retrieved successfully");
        }

        public async Task<ApiResponse<bool>> VerifyMarketerAsync(int marketerId)
        {
            var marketer = await _unitOfWork.Repository<Marketer>().GetByIdAsync(marketerId);
            if (marketer == null)
                return ApiResponse<bool>.CreateFail("Marketer not found");

            if (marketer.IsVerified)
                return ApiResponse<bool>.CreateFail("Marketer already verified");

            marketer.IsVerified = true;
            marketer.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Marketer>().Update(marketer);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, "Marketer verified successfully");
        }

        public async Task<ApiResponse<bool>> UnverifyMarketerAsync(int marketerId)
        {
            var marketer = await _unitOfWork.Repository<Marketer>().GetByIdAsync(marketerId);
            if (marketer == null)
                return ApiResponse<bool>.CreateFail("Marketer not found");

            if (!marketer.IsVerified)
                return ApiResponse<bool>.CreateFail("Marketer is not verified");

            marketer.IsVerified = false;
            marketer.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Marketer>().Update(marketer);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, "Marketer unverified successfully");
        }

        public async Task<ApiResponse<bool>> UpdatePerformanceScoreAsync(int marketerId, decimal performanceScore)
        {
            if (performanceScore < 0 || performanceScore > 5)
                return ApiResponse<bool>.CreateFail("Performance score must be between 0 and 5");

            var marketer = await _unitOfWork.Repository<Marketer>().GetByIdAsync(marketerId);
            if (marketer == null)
                return ApiResponse<bool>.CreateFail("Marketer not found");

            marketer.PerformanceScore = performanceScore;
            marketer.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Marketer>().Update(marketer);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, "Performance score updated successfully");
        }

        public async Task<ApiResponse<PagedResult<MarketerPublicDto>>> GetPendingVerificationMarketersAsync(int page = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Repository<Marketer>()
                .GetQueryable()
                .Where(m => !m.IsVerified);

            var total = await query.CountAsync();
            var marketers = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(m => m.User)
                .ToListAsync();

            var marketersDto = _mapper.Map<List<MarketerPublicDto>>(marketers);
            var pagedResult = new PagedResult<MarketerPublicDto>(marketersDto, page, pageSize, total);
            return ApiResponse<PagedResult<MarketerPublicDto>>.CreateSuccess(pagedResult, "Pending verification marketers retrieved successfully");
        }

        private string DeterminePersonalityType(int score)
        {
            return score switch
            {
                < 50 => "Introvert",
                < 100 => "Ambivert",
                _ => "Extrovert"
            };
        }

        private string GetPersonalityDescription(string personalityType)
        {
            return personalityType switch
            {
                "Introvert" => "You prefer working independently and building deep relationships.",
                "Ambivert" => "You balance between introversion and extroversion, adapting to different situations.",
                "Extrovert" => "You are outgoing and thrive on social interaction and collaboration.",
                _ => "Unknown personality type"
            };
        }
    }
}
