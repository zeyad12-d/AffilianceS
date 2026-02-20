using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.ComplaintDto;
using Affiliance_core.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Affiliance_Api.Controllers
{
    /// <summary>
    /// Controller for managing complaints including creation, resolution, and admin statistics.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ComplaintController : ControllerBase
    {
        private readonly IServicesManager _servicesManager;

        public ComplaintController(IServicesManager servicesManager)
        {
            _servicesManager = servicesManager;
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        /// <summary>
        /// Creates a new complaint (any authenticated user).
        /// </summary>
        /// <param name="dto">Complaint details including subject, description, and target user.</param>
        /// <returns>Returns the created complaint.</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateComplaint([FromBody] CreateComplaintDto dto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            if (dto is null)
                return BadRequest("Request body is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _servicesManager.ComplaintService.CreateComplaintAsync(userId.Value, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gets complaints submitted by the current user (any authenticated user).
        /// </summary>
        /// <param name="filter">Filter options such as status and pagination.</param>
        /// <returns>Returns a paginated list of the user's complaints.</returns>
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyComplaints([FromQuery] ComplaintFilterDto filter)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var result = await _servicesManager.ComplaintService.GetMyComplaintsAsync(userId.Value, filter);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gets complaints filed against the current user (any authenticated user).
        /// </summary>
        /// <param name="filter">Filter options such as status and pagination.</param>
        /// <returns>Returns a paginated list of complaints against the user.</returns>
        [HttpGet("against-me")]
        [Authorize]
        public async Task<IActionResult> GetComplaintsAgainstMe([FromQuery] ComplaintFilterDto filter)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var result = await _servicesManager.ComplaintService.GetComplaintsAgainstMeAsync(userId.Value, filter);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gets details of a specific complaint (any authenticated user, must be involved).
        /// </summary>
        /// <param name="id">Complaint ID.</param>
        /// <returns>Returns complaint details.</returns>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetComplaintDetails(int id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var result = await _servicesManager.ComplaintService.GetComplaintDetailsAsync(id, userId.Value);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gets all complaints with filters (Admin only).
        /// </summary>
        /// <param name="filter">Filter options such as status, date range, and pagination.</param>
        /// <returns>Returns a paginated list of all complaints.</returns>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllComplaints([FromQuery] ComplaintFilterDto filter)
        {
            var result = await _servicesManager.ComplaintService.GetAllComplaintsAsync(filter);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Resolves a complaint (Admin only).
        /// </summary>
        /// <param name="id">Complaint ID.</param>
        /// <param name="dto">Resolution details and notes.</param>
        /// <returns>Returns the resolution result.</returns>
        [HttpPost("{id}/resolve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResolveComplaint(int id, [FromBody] ResolveComplaintDto dto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            if (dto is null)
                return BadRequest("Request body is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _servicesManager.ComplaintService.ResolveComplaintAsync(id, userId.Value, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gets complaint statistics for the platform (Admin only).
        /// </summary>
        /// <returns>Returns aggregated complaint statistics.</returns>
        [HttpGet("admin/statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetComplaintStatistics()
        {
            var result = await _servicesManager.ComplaintService.GetComplaintStatisticsAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
    