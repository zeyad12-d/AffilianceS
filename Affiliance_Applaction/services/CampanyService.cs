using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.AccountDto;
using Affiliance_core.Dto.CampanyDto;
using Affiliance_core.Entites;
using Affiliance_core.interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace Affiliance_Applaction.services
{
    public class CampanyService : ICampanyServices
    {
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;

        private const string SuccessfulyRegisteredMessage = "تم تسجيل الشركة بنجاح، بانتظار مراجعة الإدارة.";
        private const string CompanyNotFoundMessage = "الشركة غير موجودة.";
        private const string UpdateSuccessfulMessage = "تم تحديث البيانات بنجاح.";
        private const string ApprovalSuccessfulMessage = "تم الموافقة على الشركة.";
        private const string RejectionSuccessfulMessage = "تم رفض الشركة.";
        private const string AlreadyVerifiedMessage = "الشركة موثقة بالفعل.";

        public CampanyService(UserManager<User> userManager, IUnitOfWork unitOfWork, IMapper mapper, IFileService fileService)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileService = fileService;
        }

        #region Registration
        public async Task<ApiResponse<string>> RegisterCompanyAsync(CompanyRegisterDto dto)
        {
            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Status = UserStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return ApiResponse<string>.CreateFail(string.Join(", ", result.Errors.Select(e => e.Description)));

            try
            {
                await _userManager.AddToRoleAsync(user, "Company");

                var company = _mapper.Map<Company>(dto);
                company.UserId = user.Id;

                company.CommercialRegister = await _fileService.SaveFileAsync(dto.CommercialRegisterFile, "CommercialRegisters");

                if (dto.LogoFile != null)
                {
                    company.LogoUrl = await _fileService.SaveFileAsync(dto.LogoFile, "Logos");
                }

                await _unitOfWork.Repository<Company>().AddAsync(company);
                await _unitOfWork.CompleteAsync();

                return ApiResponse<string>.CreateSuccess("Company", SuccessfulyRegisteredMessage);
            }
            catch (Exception ex)
            {
                await _userManager.DeleteAsync(user);
                return ApiResponse<string>.CreateFail("Error during company registration: " + ex.Message);
            }
        }
        #endregion

        #region Company Profile Management
        public async Task<ApiResponse<CompanyDetailsDto>> GetCompanyByIdAsync(int companyId)
        {
            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(companyId);
            if (company == null)
                return ApiResponse<CompanyDetailsDto>.CreateFail(CompanyNotFoundMessage);

            var user = await _userManager.FindByIdAsync(company.UserId.ToString());
            var companyDto = _mapper.Map<CompanyDetailsDto>(company);
            
            if (user != null)
            {
                companyDto.UserEmail = user.Email;
                companyDto.UserFirstName = user.FirstName;
                companyDto.UserLastName = user.LastName;
            }

            // Get statistics
            await PopulateCompanyStatistics(companyDto, companyId);

            return ApiResponse<CompanyDetailsDto>.CreateSuccess(companyDto, "Company details retrieved successfully");
        }

        public async Task<ApiResponse<CompanyDetailsDto>> GetMyCompanyAsync(int userId)
        {
            var companies = await _unitOfWork.Repository<Company>().FindAsync(c => c.UserId == userId);
            var company = companies.FirstOrDefault();

            if (company == null)
                return ApiResponse<CompanyDetailsDto>.CreateFail(CompanyNotFoundMessage);

            var user = await _userManager.FindByIdAsync(userId.ToString());
            var companyDto = _mapper.Map<CompanyDetailsDto>(company);

            if (user != null)
            {
                companyDto.UserEmail = user.Email;
                companyDto.UserFirstName = user.FirstName;
                companyDto.UserLastName = user.LastName;
            }

            await PopulateCompanyStatistics(companyDto, company.Id);

            return ApiResponse<CompanyDetailsDto>.CreateSuccess(companyDto, "Your company details retrieved successfully");
        }

        public async Task<ApiResponse<CompanyDto>> UpdateCompanyAsync(int companyId, UpdateCompanyDto dto)
        {
            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(companyId);
            if (company == null)
                return ApiResponse<CompanyDto>.CreateFail(CompanyNotFoundMessage);

            _mapper.Map(dto, company);
            company.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Company>().Update(company);
            await _unitOfWork.CompleteAsync();

            var resultDto = _mapper.Map<CompanyDto>(company);
            return ApiResponse<CompanyDto>.CreateSuccess(resultDto, UpdateSuccessfulMessage);
        }

        public async Task<ApiResponse<string>> UpdateCompanyLogoAsync(int companyId, IFormFile logoFile)
        {
            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(companyId);
            if (company == null)
                return ApiResponse<string>.CreateFail(CompanyNotFoundMessage);

            // Delete old logo if exists
            if (!string.IsNullOrEmpty(company.LogoUrl))
            {
                await _fileService.DeleteFileAsync(company.LogoUrl);
            }

            company.LogoUrl = await _fileService.SaveFileAsync(logoFile, "Logos");
            company.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Company>().Update(company);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<string>.CreateSuccess(company.LogoUrl, "Logo updated successfully");
        }
        #endregion

        #region Statistics & Analytics
        public async Task<ApiResponse<CompanyStatisticsDto>> GetCompanyStatisticsAsync(int companyId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(companyId);
            if (company == null)
                return ApiResponse<CompanyStatisticsDto>.CreateFail(CompanyNotFoundMessage);

            fromDate ??= DateTime.UtcNow.AddMonths(-1);
            toDate ??= DateTime.UtcNow;

            var stats = new CompanyStatisticsDto
            {
                CompanyId = companyId,
                CompanyName = company.CampanyName,
                FromDate = fromDate.Value,
                ToDate = toDate.Value
            };

            // Get campaign statistics
            var campaigns = await _unitOfWork.Repository<Campaign>().FindAsync(c => c.CompanyId == companyId);
            stats.ActiveCampaignsCount = campaigns.Count(c => c.Status == CampaignStatus.Active);
            stats.PausedCampaignsCount = campaigns.Count(c => c.Status == CampaignStatus.Paused);
            stats.CompletedCampaignsCount = campaigns.Count(c => c.Status == CampaignStatus.Completed);
            stats.RejectedCampaignsCount = campaigns.Count(c => c.Status == CampaignStatus.Rejected);

            // Get application statistics from campaigns
            long totalClicks = 0, totalConversions = 0;
            decimal totalRevenue = 0, totalCommission = 0;

            foreach (var campaign in campaigns)
            {
                var applications = await _unitOfWork.Repository<CampaignApplication>().FindAsync(a => a.CampaignId == campaign.Id);
                stats.TotalApplicationsCount += applications.Count();
                stats.ApprovedApplicationsCount += applications.Count(a => a.Status == ApplicationStatus.Accepted);
                stats.PendingApplicationsCount += applications.Count(a => a.Status == ApplicationStatus.Pending);
                stats.RejectedApplicationsCount += applications.Count(a => a.Status == ApplicationStatus.Rejected);

                var trackingLinks = await _unitOfWork.Repository<TrackingLink>().FindAsync(t => t.CampaignId == campaign.Id);
                foreach (var link in trackingLinks)
                {
                    totalClicks += link.Clicks;
                    totalConversions += link.Conversions;
                    totalRevenue += link.Earnings;
                    totalCommission += link.Earnings;
                }
            }

            stats.TotalClicks = totalClicks;
            stats.TotalConversions = totalConversions;
            stats.TotalRevenue = totalRevenue;
            stats.TotalCommissionPaid = totalCommission;
            stats.ActiveMarketersCount = campaigns.SelectMany(c => c.CampaignApplications ?? new List<CampaignApplication>())
                .Where(a => a.Status == ApplicationStatus.Accepted)
                .Select(a => a.MarketerId)
                .Distinct()
                .Count();

            if (stats.TotalClicks > 0)
                stats.AverageConversionRate = (decimal)stats.TotalConversions / stats.TotalClicks * 100;

            if (stats.TotalApplicationsCount > 0)
                stats.ApprovalRate = (decimal)stats.ApprovedApplicationsCount / stats.TotalApplicationsCount * 100;

            return ApiResponse<CompanyStatisticsDto>.CreateSuccess(stats, "Company statistics retrieved successfully");
        }
        #endregion

        #region Admin Functions
        public async Task<ApiResponse<PagedResult<CompanyApprovalDto>>> GetPendingCompaniesAsync(int page = 1, int pageSize = 10)
        {
            var users = await _userManager.GetUsersInRoleAsync("Company");
            var pendingUsers = users.Where(u => u.Status == UserStatus.Pending).ToList();

            var companies = await _unitOfWork.Repository<Company>().GetAllAsync();
            var pendingCompanies = companies
                .Where(c => pendingUsers.Any(u => u.Id == c.UserId))
                .ToList();

            var total = pendingCompanies.Count;
            var result = pendingCompanies
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var dtos = new List<CompanyApprovalDto>();
            foreach (var company in result)
            {
                var user = pendingUsers.FirstOrDefault(u => u.Id == company.UserId);
                var dto = _mapper.Map<CompanyApprovalDto>(company);
                
                if (user != null)
                {
                    dto.UserEmail = user.Email;
                    dto.UserFirstName = user.FirstName;
                    dto.UserLastName = user.LastName;
                    dto.Status = user.Status.ToString();
                }

                dtos.Add(dto);
            }

            var pagedResult = new PagedResult<CompanyApprovalDto>(dtos, page, pageSize, total);

            return ApiResponse<PagedResult<CompanyApprovalDto>>.CreateSuccess(pagedResult, "Pending companies retrieved successfully");
        }

        public async Task<ApiResponse<PagedResult<CompanyDto>>> GetVerifiedCompaniesAsync(int page = 1, int pageSize = 10)
        {
            var companies = await _unitOfWork.Repository<Company>().GetAllAsync();
            var verifiedCompanies = companies.Where(c => c.IsVerified).ToList();

            var total = verifiedCompanies.Count;
            var result = verifiedCompanies
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var dtos = _mapper.Map<List<CompanyDto>>(result);

            var pagedResult = new PagedResult<CompanyDto>(dtos, page, pageSize, total);

            return ApiResponse<PagedResult<CompanyDto>>.CreateSuccess(pagedResult, "Verified companies retrieved successfully");
        }

        public async Task<ApiResponse<CompanyDto>> ApproveCompanyAsync(int companyId, string? note = null)
        {
            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(companyId);
            if (company == null)
                return ApiResponse<CompanyDto>.CreateFail(CompanyNotFoundMessage);

            var user = await _userManager.FindByIdAsync(company.UserId.ToString());
            if (user == null)
                return ApiResponse<CompanyDto>.CreateFail("Associated user not found");

            // Update user status to Active
            user.Status = UserStatus.Active;
            company.IsVerified = true;
            company.UpdatedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);
            _unitOfWork.Repository<Company>().Update(company);
            await _unitOfWork.CompleteAsync();

            var resultDto = _mapper.Map<CompanyDto>(company);
            return ApiResponse<CompanyDto>.CreateSuccess(resultDto, ApprovalSuccessfulMessage);
        }

        public async Task<ApiResponse<string>> RejectCompanyAsync(int companyId, string rejectReason)
        {
            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(companyId);
            if (company == null)
                return ApiResponse<string>.CreateFail(CompanyNotFoundMessage);

            var user = await _userManager.FindByIdAsync(company.UserId.ToString());
            if (user == null)
                return ApiResponse<string>.CreateFail("Associated user not found");

            // Delete the company
            _unitOfWork.Repository<Company>().Delete(company);
            await _unitOfWork.CompleteAsync();

            // Delete the user
            await _userManager.DeleteAsync(user);

            return ApiResponse<string>.CreateSuccess("", RejectionSuccessfulMessage);
        }

        public async Task<ApiResponse<bool>> SuspendCompanyAsync(int companyId, string? reason = null)
        {
            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(companyId);
            if (company == null)
                return ApiResponse<bool>.CreateFail(CompanyNotFoundMessage);

            var user = await _userManager.FindByIdAsync(company.UserId.ToString());
            if (user == null)
                return ApiResponse<bool>.CreateFail("Associated user not found");

            user.Status = UserStatus.Suspended;
            await _userManager.UpdateAsync(user);

            return ApiResponse<bool>.CreateSuccess(true, "Company suspended successfully");
        }

        public async Task<ApiResponse<bool>> ReactivateCompanyAsync(int companyId)
        {
            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(companyId);
            if (company == null)
                return ApiResponse<bool>.CreateFail(CompanyNotFoundMessage);

            var user = await _userManager.FindByIdAsync(company.UserId.ToString());
            if (user == null)
                return ApiResponse<bool>.CreateFail("Associated user not found");

            user.Status = UserStatus.Active;
            await _userManager.UpdateAsync(user);

            return ApiResponse<bool>.CreateSuccess(true, "Company reactivated successfully");
        }

        public async Task<ApiResponse<bool>> VerifyCompanyAsync(int companyId)
        {
            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(companyId);
            if (company == null)
                return ApiResponse<bool>.CreateFail(CompanyNotFoundMessage);

            if (company.IsVerified)
                return ApiResponse<bool>.CreateFail(AlreadyVerifiedMessage);

            company.IsVerified = true;
            company.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Company>().Update(company);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, "Company verified successfully");
        }

        public async Task<ApiResponse<PagedResult<CompanyDto>>> GetAllCompaniesAsync(CompanyFilterDto filter)
        {
            var companies = await _unitOfWork.Repository<Company>().GetAllAsync();

            // Filter by search keyword
            if (!string.IsNullOrEmpty(filter.SearchKeyword))
            {
                companies = companies
                    .Where(c => c.CampanyName.Contains(filter.SearchKeyword, StringComparison.OrdinalIgnoreCase) ||
                                c.Address.Contains(filter.SearchKeyword, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Filter by verification status
            if (filter.IsVerified.HasValue)
            {
                companies = companies.Where(c => c.IsVerified == filter.IsVerified).ToList();
            }

            // Sort
            companies = filter.SortBy?.ToLower() switch
            {
                "name" => filter.IsDescending 
                    ? companies.OrderByDescending(c => c.CampanyName).ToList()
                    : companies.OrderBy(c => c.CampanyName).ToList(),
                "rating" => filter.IsDescending
                    ? companies.OrderByDescending(c => c.Id).ToList()
                    : companies.OrderBy(c => c.Id).ToList(),
                _ => filter.IsDescending
                    ? companies.OrderByDescending(c => c.CreatedAt).ToList()
                    : companies.OrderBy(c => c.CreatedAt).ToList()
            };

            var total = companies.Count;
            var result = companies
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var dtos = _mapper.Map<List<CompanyDto>>(result);

            var pagedResult = new PagedResult<CompanyDto>(dtos, filter.Page, filter.PageSize, total);

            return ApiResponse<PagedResult<CompanyDto>>.CreateSuccess(pagedResult, "Companies retrieved successfully");
        }
        #endregion

        #region Public Search
        public async Task<ApiResponse<PagedResult<CompanyDto>>> SearchCompaniesAsync(string keyword, int page = 1, int pageSize = 10)
        {
            var companies = await _unitOfWork.Repository<Company>().GetAllAsync();
            var filtered = companies
                .Where(c => c.CampanyName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                            c.Description?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();

            var total = filtered.Count;
            var result = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var dtos = _mapper.Map<List<CompanyDto>>(result);

            var pagedResult = new PagedResult<CompanyDto>(dtos, page, pageSize, total);

            return ApiResponse<PagedResult<CompanyDto>>.CreateSuccess(pagedResult, "Search results retrieved successfully");
        }
        #endregion

        #region Helper Methods
        private async Task PopulateCompanyStatistics(CompanyDetailsDto dto, int companyId)
        {
            var campaigns = await _unitOfWork.Repository<Campaign>().FindAsync(c => c.CompanyId == companyId);
            dto.ActiveCampaignsCount = campaigns.Count(c => c.Status == CampaignStatus.Active);
            dto.TotalCampaignsCount = campaigns.Count();

            var applications = campaigns
                .SelectMany(c => c.CampaignApplications ?? new List<CampaignApplication>())
                .ToList();
            
            dto.TotalMarketerApplicationsCount = applications.Count;
            dto.ApprovedApplicationsCount = applications.Count(a => a.Status == ApplicationStatus.Accepted);
        }
        #endregion
    }
}

