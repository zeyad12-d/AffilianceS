using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.CampaignDto;
using Affiliance_core.Dto.MarkterDto;
using Affiliance_core.Entites;
using Affiliance_core.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Affiliance_Api.Controllers
{
    /// <summary>
    /// Controller for managing campaigns including CRUD operations, applications, and statistics
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CampaignController : ControllerBase
    {
        private readonly IServicesManager _servicesManager;

        public CampaignController(IServicesManager servicesManager)
        {
            _servicesManager = servicesManager;
        }

        #region GET Endpoints - Public Campaign Discovery

        /// <summary>
        /// Get campaigns with filtering options
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CampaignDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCampaigns([FromQuery] CampaignFilterDto filter)
        {
            var result = await _servicesManager.CampaignService.GetCampaignsAsync(filter);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Get active campaigns only
        /// </summary>
        [HttpGet("active")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CampaignDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveCampaigns([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _servicesManager.CampaignService.GetActiveCampaignsAsync(page, pageSize);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Get campaign details by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CampaignDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCampaignById(int id)
        {
            var result = await _servicesManager.CampaignService.GetCampaignByIdAsync(id);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        /// <summary>
        /// Get campaigns by category
        /// </summary>
        [HttpGet("category/{categoryId}")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CampaignDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCampaignsByCategory(int categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _servicesManager.CampaignService.GetCampaignsByCategoryAsync(categoryId, page, pageSize);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Get campaigns by company
        /// </summary>
        [HttpGet("company/{companyId}")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CampaignDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCampaignsByCompany(int companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _servicesManager.CampaignService.GetCampaignsByCompanyAsync(companyId, page, pageSize);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Search campaigns with advanced filters
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CampaignDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchCampaigns([FromQuery] CampaignSearchDto searchDto)
        {
            var result = await _servicesManager.CampaignService.SearchCampaignsAsync(searchDto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Get campaigns by status
        /// </summary>
        [HttpGet("status/{status}")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CampaignDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCampaignsByStatus(CampaignStatus status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _servicesManager.CampaignService.GetCampaignsByStatusAsync(status, page, pageSize);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        #endregion

        #region GET Endpoints - Marketer Operations

        /// <summary>
        /// Get AI-recommended campaigns for current marketer
        /// </summary>
        [HttpGet("recommended")]
        [Authorize(Roles = "Marketer")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CampaignDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRecommendedCampaigns([FromQuery] int limit = 10)
        {
            var marketerId = GetCurrentMarketerId();
            if (marketerId == null)
                return Unauthorized(ApiResponse<string>.CreateFail("Marketer not found"));

            var result = await _servicesManager.CampaignService.GetRecommendedCampaignsAsync(marketerId.Value, limit);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        #endregion

        #region GET Endpoints - Company Owner

        /// <summary>
        /// Get company's own campaigns
        /// </summary>
        [HttpGet("my-campaigns")]
        [Authorize(Roles = "Company")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CampaignDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyCampaigns([FromQuery] CampaignFilterDto? filter)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(ApiResponse<string>.CreateFail("Company not found"));

            var result = await _servicesManager.CampaignService.GetMyCampaignsAsync(companyId.Value, filter);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Get company's own campaign by ID
        /// </summary>
        [HttpGet("my-campaigns/{id}")]
        [Authorize(Roles = "Company")]
        [ProducesResponseType(typeof(ApiResponse<CampaignDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyCampaignById(int id)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(ApiResponse<string>.CreateFail("Company not found"));

            var result = await _servicesManager.CampaignService.GetMyCampaignByIdAsync(id, companyId.Value);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        /// <summary>
        /// Get applications for a specific campaign (Company owner only)
        /// </summary>
        [HttpGet("{id}/applications")]
        [Authorize(Roles = "Company")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CampaignApplicationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCampaignApplications(
            int id, 
            [FromQuery] ApplicationStatus? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(ApiResponse<string>.CreateFail("Company not found"));

            var result = await _servicesManager.CampaignService.GetCampaignApplicationsAsync(id, companyId.Value, status, page, pageSize);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Get campaign statistics
        /// </summary>
        [HttpGet("{id}/statistics")]
        [Authorize(Roles = "Company")]
        [ProducesResponseType(typeof(ApiResponse<CampaignStatisticsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCampaignStatistics(
            int id,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(ApiResponse<string>.CreateFail("Company not found"));

            var result = await _servicesManager.CampaignService.GetCampaignStatisticsAsync(id, companyId.Value, from, to);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        #endregion

        #region POST Endpoints - Marketer

        /// <summary>
        /// Apply to a campaign (Marketer only)
        /// </summary>
        [HttpPost("{id}/apply")]
        [Authorize(Roles = "Marketer")]
        [ProducesResponseType(typeof(ApiResponse<CampaignApplicationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ApplyToCampaign(int id)
        {
            var marketerId = GetCurrentMarketerId();
            if (marketerId == null)
                return Unauthorized(ApiResponse<string>.CreateFail("Marketer not found"));

            var result = await _servicesManager.CampaignService.ApplyToCampaignAsync(id, marketerId.Value);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Withdraw application from campaign (Marketer only)
        /// </summary>
        [HttpPost("applications/{applicationId}/withdraw")]
        [Authorize(Roles = "Marketer")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> WithdrawApplication(int applicationId)
        {
            var marketerId = GetCurrentMarketerId();
            if (marketerId == null)
                return Unauthorized(ApiResponse<string>.CreateFail("Marketer not found"));

            var result = await _servicesManager.CampaignService.WithdrawApplicationAsync(applicationId, marketerId.Value);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        #endregion

        #region POST Endpoints - Company Owner

        /// <summary>
        /// Create new campaign (Company owner only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Company")]
        [ProducesResponseType(typeof(ApiResponse<CampaignDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCampaign([FromBody] CreateCampaignDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(ApiResponse<string>.CreateFail("Company not found"));

            var result = await _servicesManager.CampaignService.CreateCampaignAsync(dto, companyId.Value);
            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetCampaignById), new { id = result.Data.Id }, result);
        }

        /// <summary>
        /// Approve marketer application (Company owner only)
        /// </summary>
        [HttpPost("applications/{applicationId}/approve")]
        [Authorize(Roles = "Company")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ApproveApplication(int applicationId, [FromBody] CampaignApplicationActionDto? dto = null)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(ApiResponse<string>.CreateFail("Company not found"));

            var result = await _servicesManager.CampaignService.ApproveApplicationAsync(applicationId, companyId.Value, dto?.Note);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Reject marketer application (Company owner only)
        /// </summary>
        [HttpPost("applications/{applicationId}/reject")]
        [Authorize(Roles = "Company")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RejectApplication(int applicationId, [FromBody] CampaignApplicationActionDto dto)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.Note))
                return BadRequest(ApiResponse<string>.CreateFail("Rejection note is required"));

            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(ApiResponse<string>.CreateFail("Company not found"));

            var result = await _servicesManager.CampaignService.RejectApplicationAsync(applicationId, companyId.Value, dto.Note);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        #endregion

        #region PUT Endpoints - Company Owner

        /// <summary>
        /// Update campaign (Company owner only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Company")]
        [ProducesResponseType(typeof(ApiResponse<CampaignDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCampaign(int id, [FromBody] UpdateCampaignDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(ApiResponse<string>.CreateFail("Company not found"));

            var result = await _servicesManager.CampaignService.UpdateCampaignAsync(id, dto, companyId.Value);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Update campaign status (Company owner only)
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Company")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateCampaignStatus(int id, [FromBody] CampaignStatus status)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(ApiResponse<string>.CreateFail("Company not found"));

            var result = await _servicesManager.CampaignService.UpdateCampaignStatusAsync(id, status, companyId.Value);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Pause active campaign (Company owner only)
        /// </summary>
        [HttpPut("{id}/pause")]
        [Authorize(Roles = "Company")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> PauseCampaign(int id)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(ApiResponse<string>.CreateFail("Company not found"));

            var result = await _servicesManager.CampaignService.PauseCampaignAsync(id, companyId.Value);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Resume paused campaign (Company owner only)
        /// </summary>
        [HttpPut("{id}/resume")]
        [Authorize(Roles = "Company")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ResumeCampaign(int id)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(ApiResponse<string>.CreateFail("Company not found"));

            var result = await _servicesManager.CampaignService.ResumeCampaignAsync(id, companyId.Value);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        #endregion

        #region PUT Endpoints - Admin Only

        /// <summary>
        /// Approve pending campaign (Admin only)
        /// </summary>
        [HttpPut("{id}/admin/approve")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<CampaignDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ApproveCampaign(int id, [FromBody] string? responseNote = null)
        {
            var adminId = GetCurrentUserId();
            if (adminId == null)
                return Unauthorized(ApiResponse<string>.CreateFail("Admin not found"));

            var result = await _servicesManager.CampaignService.ApproveCampaignAsync(id, adminId.Value, responseNote);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Reject pending campaign (Admin only)
        /// </summary>
        [HttpPut("{id}/admin/reject")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<CampaignDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RejectCampaign(int id, [FromBody] string responseNote)
        {
            if (string.IsNullOrWhiteSpace(responseNote))
                return BadRequest(ApiResponse<string>.CreateFail("Rejection note is required"));

            var adminId = GetCurrentUserId();
            if (adminId == null)
                return Unauthorized(ApiResponse<string>.CreateFail("Admin not found"));

            var result = await _servicesManager.CampaignService.RejectCampaignAsync(id, adminId.Value, responseNote);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        #endregion

        #region DELETE Endpoints - Company Owner

        /// <summary>
        /// Delete campaign (Company owner only)
        /// Soft delete if has activity, hard delete otherwise
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Company")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteCampaign(int id)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(ApiResponse<string>.CreateFail("Company not found"));

            var result = await _servicesManager.CampaignService.DeleteCampaignAsync(id, companyId.Value);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        #endregion

        #region Helper Methods

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private int? GetCurrentCompanyId()
        {
            // Assuming company ID is stored in a custom claim
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            return int.TryParse(companyIdClaim, out var companyId) ? companyId : null;
        }

        private int? GetCurrentMarketerId()
        {
            // Assuming marketer ID is stored in a custom claim
            var marketerIdClaim = User.FindFirst("MarketerId")?.Value;
            return int.TryParse(marketerIdClaim, out var marketerId) ? marketerId : null;
        }

        #endregion
    }
}
