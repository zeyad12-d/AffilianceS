using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.NotificationDto;
using Affiliance_core.Entites;
using Affiliance_core.interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Affiliance_Applaction.services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public NotificationService(IUnitOfWork unitOfWork, IMapper _mapper)
        {
            _unitOfWork = unitOfWork;
            this._mapper = _mapper;
        }

        public async Task<ApiResponse<PagedResult<NotificationListDto>>> GetMyNotificationsAsync(int userId, NotificationFilterDto filter)
        {
            var query = _unitOfWork.Repository<Notification>()
                .GetQueryable()
                .Where(n => n.UserId == userId)
                .AsQueryable();

            if (filter.IsRead.HasValue)
                query = query.Where(n => n.IsRead == filter.IsRead.Value);

            if (!string.IsNullOrEmpty(filter.Type) && Enum.TryParse<NotificationType>(filter.Type, true, out var filterType))
                query = query.Where(n => n.Type == filterType);

            if (filter.StartDate.HasValue)
                query = query.Where(n => n.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(n => n.CreatedAt <= filter.EndDate.Value);

            var totalCount = await query.CountAsync();

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var notificationsDto = _mapper.Map<List<NotificationListDto>>(notifications);
            var pagedResult = new PagedResult<NotificationListDto>(notificationsDto, filter.Page, filter.PageSize, totalCount);

            return ApiResponse<PagedResult<NotificationListDto>>.CreateSuccess(pagedResult, "Notifications retrieved successfully");
        }

        public async Task<ApiResponse<NotificationSummaryDto>> GetNotificationSummaryAsync(int userId)
        {
            var total = await _unitOfWork.Repository<Notification>().GetQueryable().CountAsync(n => n.UserId == userId);
            var unread = await _unitOfWork.Repository<Notification>().GetQueryable().CountAsync(n => n.UserId == userId && !n.IsRead);
            var lastNotification = await _unitOfWork.Repository<Notification>()
                .GetQueryable()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => n.CreatedAt)
                .FirstOrDefaultAsync();

            var summary = new NotificationSummaryDto
            {
                TotalNotifications = total,
                UnreadCount = unread,
                LastNotificationDate = lastNotification
            };

            return ApiResponse<NotificationSummaryDto>.CreateSuccess(summary, "Notification summary retrieved successfully");
        }

        public async Task<ApiResponse<bool>> MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _unitOfWork.Repository<Notification>()
                .GetQueryable()
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
                return ApiResponse<bool>.CreateFail("Notification not found");

            notification.IsRead = true;
            _unitOfWork.Repository<Notification>().Update(notification);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, "Notification marked as read");
        }

        public async Task<ApiResponse<bool>> MarkAllAsReadAsync(int userId)
        {
            var notifications = await _unitOfWork.Repository<Notification>()
                .GetQueryable()
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                _unitOfWork.Repository<Notification>().Update(notification);
            }

            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, "All notifications marked as read");
        }

        public async Task<ApiResponse<bool>> DeleteNotificationAsync(int notificationId, int userId)
        {
            var notification = await _unitOfWork.Repository<Notification>()
                .GetQueryable()
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
                return ApiResponse<bool>.CreateFail("Notification not found");

            _unitOfWork.Repository<Notification>().Delete(notification);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, "Notification deleted successfully");
        }

        public async Task<ApiResponse<List<NotificationPreferenceDto>>> GetNotificationPreferencesAsync(int userId)
        {
            var preferences = await _unitOfWork.Repository<NotificationPreference>()
                .GetQueryable()
                .Where(np => np.UserId == userId)
                .ToListAsync();

            // Create default preferences if none exist
            if (!preferences.Any())
            {
                var defaultTypes = Enum.GetValues(typeof(NotificationType)).Cast<NotificationType>();
                foreach (var type in defaultTypes)
                {
                    var pref = new NotificationPreference
                    {
                        UserId = userId,
                        NotificationType = type,
                        IsEmailEnabled = true,
                        IsPushEnabled = true,
                        IsInAppEnabled = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Repository<NotificationPreference>().AddAsync(pref);
                }
                await _unitOfWork.CompleteAsync();

                preferences = await _unitOfWork.Repository<NotificationPreference>()
                    .GetQueryable()
                    .Where(np => np.UserId == userId)
                    .ToListAsync();
            }

            var result = _mapper.Map<List<NotificationPreferenceDto>>(preferences);
            return ApiResponse<List<NotificationPreferenceDto>>.CreateSuccess(result, "Notification preferences retrieved successfully");
        }

        public async Task<ApiResponse<NotificationPreferenceDto>> UpdateNotificationPreferenceAsync(int userId, UpdateNotificationPreferenceDto dto)
        {
            var preference = await _unitOfWork.Repository<NotificationPreference>()
                .GetQueryable()
                .FirstOrDefaultAsync(np => np.UserId == userId && np.NotificationType == dto.NotificationType);

            if (preference == null)
            {
                // Create if doesn't exist
                preference = new NotificationPreference
                {
                    UserId = userId,
                    NotificationType = dto.NotificationType,
                    IsEmailEnabled = dto.IsEmailEnabled ?? true,
                    IsPushEnabled = dto.IsPushEnabled ?? true,
                    IsInAppEnabled = dto.IsInAppEnabled ?? true,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Repository<NotificationPreference>().AddAsync(preference);
            }
            else
            {
                if (dto.IsEmailEnabled.HasValue)
                    preference.IsEmailEnabled = dto.IsEmailEnabled.Value;

                if (dto.IsPushEnabled.HasValue)
                    preference.IsPushEnabled = dto.IsPushEnabled.Value;

                if (dto.IsInAppEnabled.HasValue)
                    preference.IsInAppEnabled = dto.IsInAppEnabled.Value;

                preference.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<NotificationPreference>().Update(preference);
            }

            await _unitOfWork.CompleteAsync();

            var result = _mapper.Map<NotificationPreferenceDto>(preference);
            return ApiResponse<NotificationPreferenceDto>.CreateSuccess(result, "Notification preference updated successfully");
        }

        public async Task<ApiResponse<bool>> CreateNotificationAsync(int userId, string title, string body, string type, int? relatedId = null)
        {
            Enum.TryParse<NotificationType>(type, true, out var notificationType);

            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Body = body,
                Type = notificationType,
                RelatedId = relatedId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Notification>().AddAsync(notification);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, "Notification created successfully");
        }
    }
}
