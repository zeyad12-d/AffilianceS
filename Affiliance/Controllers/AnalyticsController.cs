using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.AnalyticsDto;
using Affiliance_core.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Affiliance_Api.Controllers
{
    /// <summary>
    /// Controller for analytics and reporting including company-level and admin platform-level analytics.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly IServicesManager _servicesManager;

        public AnalyticsController(IServicesManager servicesManager)
        {
            _servicesManager = servicesManager;
        }

        private int? GetCurrentCompanyId()
        {
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            return int.TryParse(companyIdClaim, out var companyId) ? companyId : null;
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        // Company Analytics

        /// <summary>
        /// Gets analytics overview for a company. Company users see their own data; Admin can specify a companyId.
        /// </summary>
        /// <param name="filter">Date range and grouping filters.</param>
        /// <param name="companyId">Optional company ID (Admin only). Ignored for Company users.</param>
        /// <returns>Returns company-level analytics data.</returns>
        [HttpGet("company/overview")]
        [Authorize(Roles = "Company,Admin")]
        public async Task<IActionResult> GetCompanyAnalytics([FromQuery] AnalyticsFilterDto filter, [FromQuery] int? companyId = null)
        {
            var resolvedCompanyId = IsAdmin() ? companyId : GetCurrentCompanyId();
            if (!resolvedCompanyId.HasValue)
            {
                if (IsAdmin())
                    return BadRequest(ApiResponse<string>.CreateFail("companyId query parameter is required for Admin."));
                return Unauthorized();
            }

            var result = await _servicesManager.AnalyticsService.GetCompanyAnalyticsAsync(resolvedCompanyId.Value, filter);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gets performance data of marketers working with a company. Company users see their own data; Admin can specify a companyId.
        /// </summary>
        /// <param name="filter">Date range and grouping filters.</param>
        /// <param name="companyId">Optional company ID (Admin only). Ignored for Company users.</param>
        /// <returns>Returns marketer performance metrics.</returns>
        [HttpGet("company/marketer-performance")]
        [Authorize(Roles = "Company,Admin")]
        public async Task<IActionResult> GetMarketerPerformance([FromQuery] AnalyticsFilterDto filter, [FromQuery] int? companyId = null)
        {
            var resolvedCompanyId = IsAdmin() ? companyId : GetCurrentCompanyId();
            if (!resolvedCompanyId.HasValue)
            {
                if (IsAdmin())
                    return BadRequest(ApiResponse<string>.CreateFail("companyId query parameter is required for Admin."));
                return Unauthorized();
            }

            var result = await _servicesManager.AnalyticsService.GetMarketerPerformanceAsync(resolvedCompanyId.Value, filter);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gets the conversion funnel for a specific campaign (Company or Admin).
        /// </summary>
        /// <param name="campaignId">The campaign ID.</param>
        /// <param name="filter">Date range filters.</param>
        /// <returns>Returns conversion funnel stages data.</returns>
        [HttpGet("company/conversion-funnel/{campaignId}")]
        [Authorize(Roles = "Company,Admin")]
        public async Task<IActionResult> GetConversionFunnel(int campaignId, [FromQuery] AnalyticsFilterDto filter)
        {
            var result = await _servicesManager.AnalyticsService.GetConversionFunnelAsync(campaignId, filter);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // Admin Analytics

        /// <summary>
        /// Gets platform-wide overview analytics (Admin only).
        /// </summary>
        /// <returns>Returns overall platform statistics.</returns>
        [HttpGet("admin/platform-overview")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPlatformOverview()
        {
            var result = await _servicesManager.AnalyticsService.GetPlatformOverviewAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gets revenue breakdown analytics (Admin only).
        /// </summary>
        /// <param name="filter">Date range and grouping filters.</param>
        /// <returns>Returns revenue breakdown data.</returns>
        [HttpGet("admin/revenue-breakdown")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRevenueBreakdown([FromQuery] AnalyticsFilterDto filter)
        {
            var result = await _servicesManager.AnalyticsService.GetRevenueBreakdownAsync(filter);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gets top-performing marketers on the platform (Admin only).
        /// </summary>
        /// <param name="topCount">Number of top performers to return (default: 10).</param>
        /// <returns>Returns a ranked list of top-performing marketers.</returns>
        [HttpGet("admin/top-performers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTopPerformers([FromQuery] int topCount = 10)
        {
            var result = await _servicesManager.AnalyticsService.GetTopPerformersAsync(topCount);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
