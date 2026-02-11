using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.MarkterDto;
using Affiliance_core.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Affiliance.Controllers
{
    /// <summary>
    /// Controller for managing marketer operations including dashboard, profile, applications, and statistics.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]

    public class MarketerController : ControllerBase
    {
        private readonly IServicesManager _servicesManager;

        public MarketerController(IServicesManager servicesManager)
        {
            _servicesManager = servicesManager;
        }

        /// <summary>
        /// Retrieves the dashboard data for the authenticated marketer.
        /// </summary>
        /// <returns>Returns the marketer's dashboard statistics and recent activities.</returns>
        [HttpGet("my/dashboard")]
        [Authorize(Roles = "Marketer")]
        [ProducesResponseType(typeof(ApiResponse<MarketerDashboardDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDashboard()
        {
            var marketerId = GetMarketerId();
            if (marketerId == 0)
                return Unauthorized();

            var result = await _servicesManager.MarketerService.GetDashboardAsync(marketerId);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves statistics for the authenticated marketer within a specified date range.
        /// </summary>
        /// <param name="startDate">The start date for the statistics filter (optional).</param>
        /// <param name="endDate">The end date for the statistics filter (optional).</param>
        /// <returns>Returns statistics including conversions, clicks, and earnings.</returns>
        [HttpGet("my/statistics")]
        [Authorize(Roles = "Marketer")]
        [ProducesResponseType(typeof(ApiResponse<MarketerStatisticsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatistics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var marketerId = GetMarketerId();
            if (marketerId == 0)
                return Unauthorized();

            var result = await _servicesManager.MarketerService.GetStatisticsAsync(marketerId, startDate, endDate);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves the earnings report for the authenticated marketer.
        /// </summary>
        /// <param name="startDate">The start date for the report (optional).</param>
        /// <param name="endDate">The end date for the report (optional).</param>
        /// <param name="groupBy">The grouping interval (e.g., "month", "week", "day"). Default is "month".</param>
        /// <returns>Returns a report of earnings grouped by the specified interval.</returns>
        [HttpGet("my/earnings-report")]
        [Authorize(Roles = "Marketer")]
        [ProducesResponseType(typeof(ApiResponse<EarningsReportDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEarningsReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? groupBy = "month")
        {
            var marketerId = GetMarketerId();
            if (marketerId == 0)
                return Unauthorized();

            var result = await _servicesManager.MarketerService.GetEarningsReportAsync(marketerId, startDate, endDate, groupBy);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves the performance history of the authenticated marketer.
        /// </summary>
        /// <returns>Returns a list of performance scores over time.</returns>
        [HttpGet("my/performance-history")]
        [Authorize(Roles = "Marketer")]
        [ProducesResponseType(typeof(ApiResponse<List<PerformanceHistoryDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPerformanceHistory()
        {
            var marketerId = GetMarketerId();
            if (marketerId == 0)
                return Unauthorized();

            var result = await _servicesManager.MarketerService.GetPerformanceHistoryAsync(marketerId);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a paginated list of campaign applications for the authenticated marketer.
        /// </summary>
        /// <param name="filter">The filter criteria including status, campaign ID, and pagination.</param>
        /// <returns>Returns a paginated list of campaign applications.</returns>
        [HttpGet("my/applications")]
        [Authorize(Roles = "Marketer")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CampaignApplicationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyApplications([FromQuery] ApplicationFilterDto? filter)
        {
            var marketerId = GetMarketerId();
            if (marketerId == 0)
                return Unauthorized();

            var result = await _servicesManager.MarketerService.GetMyApplicationsAsync(marketerId, filter);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific campaign application by its ID.
        /// </summary>
        /// <param name="applicationId">The ID of the application to retrieve.</param>
        /// <returns>Returns the detailed information of the campaign application.</returns>
        [HttpGet("my/applications/{applicationId}")]
        [Authorize(Roles = "Marketer")]
        [ProducesResponseType(typeof(ApiResponse<CampaignApplicationDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetApplicationById(int applicationId)
        {
            var marketerId = GetMarketerId();
            if (marketerId == 0)
                return Unauthorized();

            var result = await _servicesManager.MarketerService.GetApplicationByIdAsync(applicationId, marketerId);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }


        /// <summary>
        /// Retrieves AI-powered campaign suggestions for the authenticated marketer.
        /// </summary>
        /// <param name="limit">The maximum number of suggestions to return (default is 10).</param>
        /// <returns>Returns a list of suggested campaigns based on marketer profile and history.</returns>
        [HttpGet("my/ai-suggestions")]
        [Authorize(Roles = "Marketer")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<AiSuggestionDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAiSuggestions([FromQuery] int limit = 10)
        {
            var marketerId = GetMarketerId();
            if (marketerId == 0)
                return Unauthorized();

            var result = await _servicesManager.MarketerService.GetAiSuggestionsAsync(marketerId, limit);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves the results of the personality test for the authenticated marketer.
        /// </summary>
        /// <returns>Returns the personality test result/score.</returns>
        [HttpGet("my/personality-test")]
        [Authorize(Roles = "Marketer")]
        [ProducesResponseType(typeof(ApiResponse<PersonalityTestResultDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPersonalityTestResults()
        {
            var marketerId = GetMarketerId();
            if (marketerId == 0)
                return Unauthorized();

            var result = await _servicesManager.MarketerService.GetPersonalityTestResultsAsync(marketerId);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        

        #region PUT Endpoints (Authenticated)

        /// <summary>
        /// Updates the authenticated marketer's profile information.
        /// </summary>
        /// <param name="dto">The profile update data containing bio, niche, social links, etc.</param>
        /// <returns>Returns the updated profile data.</returns>
        [HttpPut("my/profile")]
        [Authorize(Roles = "Marketer")]
        [ProducesResponseType(typeof(ApiResponse<MarketerProfileDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateMarketerProfileDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var marketerId = GetMarketerId();
            if (marketerId == 0)
                return Unauthorized();

            var result = await _servicesManager.MarketerService.UpdateProfileAsync(marketerId, dto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Updates the authenticated marketer's CV file.
        /// </summary>
        /// <param name="cvFile">The new CV file to upload.</param>
        /// <returns>Returns the URL or path of the uploaded CV.</returns>
        [HttpPut("my/cv")]
        [Authorize(Roles = "Marketer")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateCv(IFormFile cvFile)
        {
            var marketerId = GetMarketerId();
            if (marketerId == 0)
                return Unauthorized();

            var result = await _servicesManager.MarketerService.UpdateCvAsync(marketerId, cvFile);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Updates the authenticated marketer's National ID document.
        /// </summary>
        /// <param name="nationalIdFile">The new National ID file to upload.</param>
        /// <returns>Returns the URL or path of the uploaded document.</returns>
        [HttpPut("my/national-id")]
        [Authorize(Roles = "Marketer")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateNationalId(IFormFile nationalIdFile)
        {
            var marketerId = GetMarketerId();
            if (marketerId == 0)
                return Unauthorized();

            var result = await _servicesManager.MarketerService.UpdateNationalIdAsync(marketerId, nationalIdFile);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Updates the skills list for the authenticated marketer.
        /// </summary>
        /// <param name="skills">A string representation of the skills.</param>
        /// <returns>Returns true if update was successful.</returns>
        [HttpPut("my/skills")]
        [Authorize(Roles = "Marketer")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateSkills([FromBody] string skills)
        {
            var marketerId = GetMarketerId();
            if (marketerId == 0)
                return Unauthorized();

            var result = await _servicesManager.MarketerService.UpdateSkillsAsync(marketerId, skills);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Updates the bio for the authenticated marketer.
        /// </summary>
        /// <param name="bio">The new bio text.</param>
        /// <returns>Returns true if update was successful.</returns>
        [HttpPut("my/bio")]
        [Authorize(Roles = "Marketer")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateBio([FromBody] string bio)
        {
            var marketerId = GetMarketerId();
            if (marketerId == 0)
                return Unauthorized();

            var result = await _servicesManager.MarketerService.UpdateBioAsync(marketerId, bio);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Updates the niche for the authenticated marketer.
        /// </summary>
        /// <param name="niche">The new niche identifier or name.</param>
        /// <returns>Returns true if update was successful.</returns>
        [HttpPut("my/niche")]
        [Authorize(Roles = "Marketer")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateNiche([FromBody] string niche)
        {
            var marketerId = GetMarketerId();
            if (marketerId == 0)
                return Unauthorized();

            var result = await _servicesManager.MarketerService.UpdateNicheAsync(marketerId, niche);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Updates the social media links for the authenticated marketer.
        /// </summary>
        /// <param name="socialLinks">A string representation or JSON of social links.</param>
        /// <returns>Returns true if update was successful.</returns>
        [HttpPut("my/social-links")]
        [Authorize(Roles = "Marketer")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateSocialLinks([FromBody] string socialLinks)
        {
            var marketerId = GetMarketerId();
            if (marketerId == 0)
                return Unauthorized();

            var result = await _servicesManager.MarketerService.UpdateSocialLinksAsync(marketerId, socialLinks);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        #endregion

        #region POST Endpoints (Authenticated)

        /// <summary>
        /// Submits a personality test responses for the authenticated marketer.
        /// </summary>
        /// <param name="testDto">The personality test responses.</param>
        /// <returns>Returns the result of the personality test.</returns>
        [HttpPost("my/personality-test")]
        [Authorize(Roles = "Marketer")]
        [ProducesResponseType(typeof(ApiResponse<PersonalityTestResultDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SubmitPersonalityTest([FromBody] PersonalityTestDto testDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var marketerId = GetMarketerId();
            if (marketerId == 0)
                return Unauthorized();

            var result = await _servicesManager.MarketerService.SubmitPersonalityTestAsync(marketerId, testDto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Withdraws a submitted campaign application.
        /// </summary>
        /// <param name="applicationId">The ID of the application to withdraw.</param>
        /// <returns>Returns true if withdrawal was successful.</returns>
        [HttpPost("my/applications/{applicationId}/withdraw")]
        [Authorize(Roles = "Marketer")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> WithdrawApplication(int applicationId)
        {
            var marketerId = GetMarketerId();
            if (marketerId == 0)
                return Unauthorized();

            var result = await _servicesManager.MarketerService.WithdrawApplicationAsync(applicationId, marketerId);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        #endregion

        #region Admin Endpoints

        /// <summary>
        /// Verifies a marketer account (Admin only).
        /// </summary>
        /// <param name="marketerId">The ID of the marketer to verify.</param>
        /// <returns>Returns true if verification was successful.</returns>
        [HttpPut("{marketerId}/verify")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> VerifyMarketer(int marketerId)
        {
            var result = await _servicesManager.MarketerService.VerifyMarketerAsync(marketerId);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Unverifies a marketer account (Admin only).
        /// </summary>
        /// <param name="marketerId">The ID of the marketer to unverify.</param>
        /// <returns>Returns true if unverification was successful.</returns>
        [HttpPut("{marketerId}/unverify")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UnverifyMarketer(int marketerId)
        {
            var result = await _servicesManager.MarketerService.UnverifyMarketerAsync(marketerId);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Updates the performance score for a marketer (Admin only).
        /// </summary>
        /// <param name="marketerId">The ID of the marketer.</param>
        /// <param name="performanceScore">The new performance score value.</param>
        /// <returns>Returns true if update was successful.</returns>
        [HttpPut("{marketerId}/performance-score")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdatePerformanceScore(int marketerId, [FromBody] decimal performanceScore)
        {
            var result = await _servicesManager.MarketerService.UpdatePerformanceScoreAsync(marketerId, performanceScore);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a paginated list of marketers pending verification (Admin only).
        /// </summary>
        /// <param name="page">Page number.</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <returns>Returns a paginated list of marketers waiting for verification.</returns>
        [HttpGet("admin/pending-verification")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<MarketerPublicDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPendingVerificationMarketers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _servicesManager.MarketerService.GetPendingVerificationMarketersAsync(page, pageSize);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        #endregion

        private int GetMarketerId()
        {
            var marketerId = User.FindFirst("marketerId")?.Value;
            return int.TryParse(marketerId, out var id) ? id : 0;
        }
    }
}
