using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.NotificationDto;
using Affiliance_core.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Affiliance_Api.Controllers
{
    /// <summary>
    /// Controller for managing user notifications and preferences (authenticated users).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly IServicesManager _servicesManager;

        public NotificationController(IServicesManager servicesManager)
        {
            _servicesManager = servicesManager;
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        /// <summary>
        /// Gets notifications for the current user (authenticated users).
        /// </summary>
        /// <param name="filter">Filter options such as read status and pagination.</param>
        /// <returns>Returns a paginated list of notifications.</returns>
        [HttpGet("my")]
        public async Task<IActionResult> GetMyNotifications([FromQuery] NotificationFilterDto filter)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var result = await _servicesManager.NotificationService.GetMyNotificationsAsync(userId.Value, filter);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gets a notification summary for the current user (authenticated users).
        /// </summary>
        /// <returns>Returns summary counts such as unread totals.</returns>
        [HttpGet("summary")]
        public async Task<IActionResult> GetNotificationSummary()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var result = await _servicesManager.NotificationService.GetNotificationSummaryAsync(userId.Value);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Marks a notification as read (authenticated users).
        /// </summary>
        /// <param name="id">Notification ID.</param>
        /// <returns>Returns the update result.</returns>
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var result = await _servicesManager.NotificationService.MarkAsReadAsync(id, userId.Value);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Marks all notifications as read (authenticated users).
        /// </summary>
        /// <returns>Returns the update result.</returns>
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var result = await _servicesManager.NotificationService.MarkAllAsReadAsync(userId.Value);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Deletes a notification (authenticated users).
        /// </summary>
        /// <param name="id">Notification ID.</param>
        /// <returns>Returns the deletion result.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var result = await _servicesManager.NotificationService.DeleteNotificationAsync(id, userId.Value);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gets notification preferences for the current user (authenticated users).
        /// </summary>
        /// <returns>Returns the current notification preference settings.</returns>
        [HttpGet("preferences")]
        public async Task<IActionResult> GetNotificationPreferences()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var result = await _servicesManager.NotificationService.GetNotificationPreferencesAsync(userId.Value);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Updates notification preferences for the current user (authenticated users).
        /// </summary>
        /// <param name="dto">Preference update payload.</param>
        /// <returns>Returns the update result.</returns>
        [HttpPut("preferences")]
        public async Task<IActionResult> UpdateNotificationPreference([FromBody] UpdateNotificationPreferenceDto dto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var result = await _servicesManager.NotificationService.UpdateNotificationPreferenceAsync(userId.Value, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
