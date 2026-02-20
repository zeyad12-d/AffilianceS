using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.AuditDto;
using Affiliance_core.Entites;
using Affiliance_core.interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Affiliance_Applaction.services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AuditLogService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<bool>> LogActionAsync(int? userId, string action, string? entityType = null, int? entityId = null, 
            string? oldValues = null, string? newValues = null, string? ipAddress = null, string? userAgent = null)
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues,
                NewValues = newValues,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow
            };

            await _unitOfWork.Repository<AuditLog>().AddAsync(auditLog);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, "Audit log created successfully");
        }

        public async Task<ApiResponse<PagedResult<AuditLogDto>>> GetAuditLogsAsync(AuditLogFilterDto filter)
        {
            var query = _unitOfWork.Repository<AuditLog>()
                .GetQueryable()
                .Include(a => a.User)
                .AsQueryable();

            if (filter.UserId.HasValue)
                query = query.Where(a => a.UserId == filter.UserId.Value);

            if (!string.IsNullOrEmpty(filter.Action))
                query = query.Where(a => a.Action.Contains(filter.Action));

            if (!string.IsNullOrEmpty(filter.EntityType))
                query = query.Where(a => a.EntityType == filter.EntityType);

            if (filter.EntityId.HasValue)
                query = query.Where(a => a.EntityId == filter.EntityId.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(a => a.Timestamp >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(a => a.Timestamp <= filter.EndDate.Value);

            if (!string.IsNullOrEmpty(filter.IpAddress))
                query = query.Where(a => a.IpAddress == filter.IpAddress);

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var logsDto = _mapper.Map<List<AuditLogDto>>(logs);
            var pagedResult = new PagedResult<AuditLogDto>(logsDto, filter.Page, filter.PageSize, totalCount);

            return ApiResponse<PagedResult<AuditLogDto>>.CreateSuccess(pagedResult, "Audit logs retrieved successfully");
        }

        public async Task<ApiResponse<List<ActivityHistoryDto>>> GetActivityHistoryAsync(int userId, int days = 30)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);

            var logs = await _unitOfWork.Repository<AuditLog>()
                .GetQueryable()
                .Where(a => a.UserId == userId && a.Timestamp >= startDate)
                .OrderByDescending(a => a.Timestamp)
                .Take(100)
                .ToListAsync();

            var activity = logs.Select(log => new ActivityHistoryDto
            {
                Action = log.Action,
                EntityType = log.EntityType,
                EntityName = log.NewValues, // Simplified
                Timestamp = log.Timestamp,
                Description = $"{log.Action} on {log.EntityType}"
            }).ToList();

            return ApiResponse<List<ActivityHistoryDto>>.CreateSuccess(activity, "Activity history retrieved successfully");
        }

        public async Task<ApiResponse<List<AuditLogDto>>> GetEntityHistoryAsync(string entityType, int entityId)
        {
            var logs = await _unitOfWork.Repository<AuditLog>()
                .GetQueryable()
                .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                .Include(a => a.User)
                .OrderBy(a => a.Timestamp)
                .ToListAsync();

            var logsDto = _mapper.Map<List<AuditLogDto>>(logs);
            return ApiResponse<List<AuditLogDto>>.CreateSuccess(logsDto, "Entity history retrieved successfully");
        }
    }
}
