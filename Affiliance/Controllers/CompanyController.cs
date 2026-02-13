using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.CampanyDto;
using Affiliance_core.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Affiliance_Api.Controllers
{
    /// <summary>
    /// Controller for managing company operations including profile, statistics, and admin approvals
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly IServicesManager _servicesManager;

        public CompanyController(IServicesManager servicesManager)
        {
            _servicesManager = servicesManager;
        }

        #region Helper Methods
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
        #endregion

        #region GET Endpoints - Public

        /// <summary>
        /// Get company details by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CompanyDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCompanyById(int id)
        {
            var result = await _servicesManager.CampanyServices.GetCompanyByIdAsync(id);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        /// <summary>
        /// Search companies by keyword
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CompanyDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchCompanies([FromQuery] string keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _servicesManager.CampanyServices.SearchCompaniesAsync(keyword, page, pageSize);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        #endregion

        #region GET Endpoints - Company (Authenticated)

        /// <summary>
        /// Get my company profile (Company owner only)
        /// </summary>
        [HttpGet("my-profile")]
        [Authorize(Roles = "Company")]
        [ProducesResponseType(typeof(ApiResponse<CompanyDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(ApiResponse<string>.CreateFail("User not found"));

            var result = await _servicesManager.CampanyServices.GetMyCompanyAsync(userId.Value);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Get my company statistics (Company owner only)
        /// </summary>
        [HttpGet("my-statistics")]
        [Authorize(Roles = "Company")]
        [ProducesResponseType(typeof(ApiResponse<CompanyStatisticsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyStatistics([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(ApiResponse<string>.CreateFail("User not found"));

            // Get company ID from user
            var companyResult = await _servicesManager.CampanyServices.GetMyCompanyAsync(userId.Value);
            if (!companyResult.Success)
                return BadRequest(companyResult);

            var result = await _servicesManager.CampanyServices.GetCompanyStatisticsAsync(companyResult.Data.Id, from, to);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        #endregion

        #region PUT Endpoints - Company (Authenticated)

        /// <summary>
        /// Update my company information (Company owner only)
        /// </summary>
        [HttpPut("my-profile")]
        [Authorize(Roles = "Company")]
        [ProducesResponseType(typeof(ApiResponse<CompanyDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateCompanyDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(ApiResponse<string>.CreateFail("User not found"));

            // Get company ID
            var companyResult = await _servicesManager.CampanyServices.GetMyCompanyAsync(userId.Value);
            if (!companyResult.Success)
                return BadRequest(companyResult);

            var result = await _servicesManager.CampanyServices.UpdateCompanyAsync(companyResult.Data.Id, dto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Update my company logo (Company owner only)
        /// </summary>
        [HttpPut("my-logo")]
        [Authorize(Roles = "Company")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateMyLogo(IFormFile logoFile)
        {
            if (logoFile == null || logoFile.Length == 0)
                return BadRequest(ApiResponse<string>.CreateFail("No file uploaded"));

            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(ApiResponse<string>.CreateFail("User not found"));

            // Get company ID
            var companyResult = await _servicesManager.CampanyServices.GetMyCompanyAsync(userId.Value);
            if (!companyResult.Success)
                return BadRequest(companyResult);

            var result = await _servicesManager.CampanyServices.UpdateCompanyLogoAsync(companyResult.Data.Id, logoFile);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        #endregion

        #region GET Endpoints - Admin

        /// <summary>
        /// Get all pending companies (for approval) - Admin only
        /// </summary>
        [HttpGet("admin/pending")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CompanyApprovalDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPendingCompanies([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _servicesManager.CampanyServices.GetPendingCompaniesAsync(page, pageSize);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Get all verified companies - Admin only
        /// </summary>
        [HttpGet("admin/verified")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CompanyDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVerifiedCompanies([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _servicesManager.CampanyServices.GetVerifiedCompaniesAsync(page, pageSize);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Get all companies with filters - Admin only
        /// </summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CompanyDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCompanies([FromQuery] CompanyFilterDto filter)
        {
            var result = await _servicesManager.CampanyServices.GetAllCompaniesAsync(filter);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        #endregion

        #region POST Endpoints - Admin

        /// <summary>
        /// Approve company (Admin only)
        /// </summary>
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<CompanyDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveCompany(int id, [FromBody] CompanyActionDto? dto = null)
        {
            var result = await _servicesManager.CampanyServices.ApproveCompanyAsync(id, dto?.Note);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        /// <summary>
        /// Reject company (Admin only)
        /// </summary>
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RejectCompany(int id, [FromBody] CompanyActionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ResponseNotes))
                return BadRequest(ApiResponse<string>.CreateFail("Rejection reason is required"));

            var result = await _servicesManager.CampanyServices.RejectCompanyAsync(id, dto.ResponseNotes);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        #endregion

        #region PUT Endpoints - Admin

        /// <summary>
        /// Verify company documents (Admin only)
        /// </summary>
        [HttpPut("{id}/verify")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VerifyCompany(int id)
        {
            var result = await _servicesManager.CampanyServices.VerifyCompanyAsync(id);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        /// <summary>
        /// Suspend company (Admin only)
        /// </summary>
        [HttpPut("{id}/suspend")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SuspendCompany(int id, [FromBody] CompanyActionDto? dto = null)
        {
            var result = await _servicesManager.CampanyServices.SuspendCompanyAsync(id, dto?.Note);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        /// <summary>
        /// Reactivate company (Admin only)
        /// </summary>
        [HttpPut("{id}/reactivate")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReactivateCompany(int id)
        {
            var result = await _servicesManager.CampanyServices.ReactivateCompanyAsync(id);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        #endregion
    }
}
