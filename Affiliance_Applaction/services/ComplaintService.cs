using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.ComplaintDto;
using Affiliance_core.Entites;
using Affiliance_core.interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Affiliance_Applaction.services
{
    public class ComplaintService : IComplaintService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IAuditLogService _auditLogService;

        public ComplaintService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService, IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _auditLogService = auditLogService;
        }

        public async Task<ApiResponse<ComplaintDetailsDto>> CreateComplaintAsync(int complainantId, CreateComplaintDto dto)
        {
            var complainant = await _unitOfWork.Repository<User>().GetByIdAsync(complainantId);
            if (complainant == null)
                return ApiResponse<ComplaintDetailsDto>.CreateFail("Complainant not found");

            var defendant = await _unitOfWork.Repository<User>().GetByIdAsync(dto.DefendantId);
            if (defendant == null)
                return ApiResponse<ComplaintDetailsDto>.CreateFail("Defendant not found");

            var complaint = new Complaint
            {
                ComplainantId = complainantId,
                DefendantId = dto.DefendantId,
                CampaignId = dto.CampaignId,
                Subject = dto.Subject,
                Description = dto.Description,
                Evidence = dto.Evidence,
                Status = ComplaintStatus.Open,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Complaint>().AddAsync(complaint);
            await _unitOfWork.CompleteAsync();

            // Notify defendant
            await _notificationService.CreateNotificationAsync(
                dto.DefendantId,
                "New Complaint Filed",
                $"A complaint has been filed against you: {dto.Subject}",
                "ComplaintUpdate",
                complaint.Id
            );

            // Reload with navigation properties
            complaint = await _unitOfWork.Repository<Complaint>()
                .GetQueryable()
                .Include(c => c.Complainant)
                .Include(c => c.Defendant)
                .Include(c => c.Campaign)
                .FirstOrDefaultAsync(c => c.Id == complaint.Id);

            var result = _mapper.Map<ComplaintDetailsDto>(complaint);
            return ApiResponse<ComplaintDetailsDto>.CreateSuccess(result, "Complaint created successfully");
        }

        public async Task<ApiResponse<PagedResult<ComplaintDto>>> GetMyComplaintsAsync(int userId, ComplaintFilterDto filter)
        {
            var query = _unitOfWork.Repository<Complaint>()
                .GetQueryable()
                .Where(c => c.ComplainantId == userId)
                .Include(c => c.Complainant)
                .Include(c => c.Defendant)
                .Include(c => c.Campaign)
                .Include(c => c.ResolvedByNavigation)
                .AsQueryable();

            return await ApplyFiltersAndPaginate(query, filter);
        }

        public async Task<ApiResponse<PagedResult<ComplaintDto>>> GetComplaintsAgainstMeAsync(int userId, ComplaintFilterDto filter)
        {
            var query = _unitOfWork.Repository<Complaint>()
                .GetQueryable()
                .Where(c => c.DefendantId == userId)
                .Include(c => c.Complainant)
                .Include(c => c.Defendant)
                .Include(c => c.Campaign)
                .Include(c => c.ResolvedByNavigation)
                .AsQueryable();

            return await ApplyFiltersAndPaginate(query, filter);
        }

        public async Task<ApiResponse<ComplaintDetailsDto>> GetComplaintDetailsAsync(int complaintId, int userId)
        {
            var complaint = await _unitOfWork.Repository<Complaint>()
                .GetQueryable()
                .Include(c => c.Complainant)
                .Include(c => c.Defendant)
                .Include(c => c.Campaign)
                .Include(c => c.ResolvedByNavigation)
                .FirstOrDefaultAsync(c => c.Id == complaintId && (c.ComplainantId == userId || c.DefendantId == userId));

            if (complaint == null)
                return ApiResponse<ComplaintDetailsDto>.CreateFail("Complaint not found or access denied");

            var result = _mapper.Map<ComplaintDetailsDto>(complaint);
            return ApiResponse<ComplaintDetailsDto>.CreateSuccess(result, "Complaint details retrieved successfully");
        }

        public async Task<ApiResponse<PagedResult<ComplaintDetailsDto>>> GetAllComplaintsAsync(ComplaintFilterDto filter)
        {
            var query = _unitOfWork.Repository<Complaint>()
                .GetQueryable()
                .Include(c => c.Complainant)
                .Include(c => c.Defendant)
                .Include(c => c.Campaign)
                .Include(c => c.ResolvedByNavigation)
                .AsQueryable();

            // Apply filters
            if (filter.ComplainantId.HasValue)
                query = query.Where(c => c.ComplainantId == filter.ComplainantId.Value);

            if (filter.DefendantId.HasValue)
                query = query.Where(c => c.DefendantId == filter.DefendantId.Value);

            if (filter.CampaignId.HasValue)
                query = query.Where(c => c.CampaignId == filter.CampaignId.Value);

            if (filter.Status.HasValue)
                query = query.Where(c => c.Status == filter.Status.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(c => c.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(c => c.CreatedAt <= filter.EndDate.Value);

            if (!string.IsNullOrEmpty(filter.SearchTerm))
                query = query.Where(c => c.Subject.Contains(filter.SearchTerm) || c.Description.Contains(filter.SearchTerm));

            var totalCount = await query.CountAsync();

            var complaints = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var complaintsDto = _mapper.Map<List<ComplaintDetailsDto>>(complaints);
            var pagedResult = new PagedResult<ComplaintDetailsDto>(complaintsDto, filter.Page, filter.PageSize, totalCount);

            return ApiResponse<PagedResult<ComplaintDetailsDto>>.CreateSuccess(pagedResult, "All complaints retrieved successfully");
        }

        public async Task<ApiResponse<ComplaintDetailsDto>> ResolveComplaintAsync(int complaintId, int adminId, ResolveComplaintDto dto)
        {
            var complaint = await _unitOfWork.Repository<Complaint>()
                .GetQueryable()
                .Include(c => c.Complainant)
                .Include(c => c.Defendant)
                .FirstOrDefaultAsync(c => c.Id == complaintId);

            if (complaint == null)
                return ApiResponse<ComplaintDetailsDto>.CreateFail("Complaint not found");

            complaint.Status = dto.Status;
            complaint.ResolutionNote = dto.ResolutionNote;
            complaint.ResolvedBy = adminId;
            complaint.CreatedAt = DateTime.UtcNow; // Should be ResolvedAt but entity doesn't have it

            _unitOfWork.Repository<Complaint>().Update(complaint);
            await _unitOfWork.CompleteAsync();

            // Log audit
            await _auditLogService.LogActionAsync(
                adminId,
                "Complaint Resolved",
                "Complaint",
                complaintId,
                null,
                $"Status: {dto.Status}, Resolution: {dto.ResolutionNote}",
                null,
                null
            );

            // Notify both parties
            await _notificationService.CreateNotificationAsync(
                complaint.ComplainantId,
                "Complaint Resolved",
                $"Your complaint has been resolved: {dto.ResolutionNote}",
                "ComplaintUpdate",
                complaintId
            );

            await _notificationService.CreateNotificationAsync(
                complaint.DefendantId,
                "Complaint Resolved",
                $"A complaint against you has been resolved: {dto.ResolutionNote}",
                "ComplaintUpdate",
                complaintId
            );

            var result = _mapper.Map<ComplaintDetailsDto>(complaint);
            return ApiResponse<ComplaintDetailsDto>.CreateSuccess(result, "Complaint resolved successfully");
        }

        public async Task<ApiResponse<object>> GetComplaintStatisticsAsync()
        {
            var total = await _unitOfWork.Repository<Complaint>().GetQueryable().CountAsync();
            var open = await _unitOfWork.Repository<Complaint>().GetQueryable().CountAsync(c => c.Status == ComplaintStatus.Open);
            var inReview = await _unitOfWork.Repository<Complaint>().GetQueryable().CountAsync(c => c.Status == ComplaintStatus.InReview);
            var resolved = await _unitOfWork.Repository<Complaint>().GetQueryable().CountAsync(c => c.Status == ComplaintStatus.Resolved);
            var dismissed = await _unitOfWork.Repository<Complaint>().GetQueryable().CountAsync(c => c.Status == ComplaintStatus.Dismissed);

            var stats = new
            {
                TotalComplaints = total,
                OpenComplaints = open,
                InReviewComplaints = inReview,
                ResolvedComplaints = resolved,
                DismissedComplaints = dismissed
            };

            return ApiResponse<object>.CreateSuccess(stats, "Complaint statistics retrieved successfully");
        }

        private async Task<ApiResponse<PagedResult<ComplaintDto>>> ApplyFiltersAndPaginate(IQueryable<Complaint> query, ComplaintFilterDto filter)
        {
            // Apply filters
            if (filter.Status.HasValue)
                query = query.Where(c => c.Status == filter.Status.Value);

            if (filter.CampaignId.HasValue)
                query = query.Where(c => c.CampaignId == filter.CampaignId.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(c => c.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(c => c.CreatedAt <= filter.EndDate.Value);

            if (!string.IsNullOrEmpty(filter.SearchTerm))
                query = query.Where(c => c.Subject.Contains(filter.SearchTerm) || c.Description.Contains(filter.SearchTerm));

            var totalCount = await query.CountAsync();

            var complaints = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var complaintsDto = _mapper.Map<List<ComplaintDto>>(complaints);
            var pagedResult = new PagedResult<ComplaintDto>(complaintsDto, filter.Page, filter.PageSize, totalCount);

            return ApiResponse<PagedResult<ComplaintDto>>.CreateSuccess(pagedResult, "Complaints retrieved successfully");
        }
    }
}
