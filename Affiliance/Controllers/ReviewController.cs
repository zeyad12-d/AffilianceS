using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.ReviewDto;
using Affiliance_core.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Affiliance_Api.Controllers
{
    /// <summary>
    /// Controller for managing marketer review operations.
    /// </summary>
    [Route("api/marketer")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IServicesManager _servicesManager;

        public ReviewController(IServicesManager servicesManager)
        {
            _servicesManager = servicesManager;
        }

        /// <summary>
        /// Retrieves reviews received by the authenticated marketer.
        /// </summary>
        /// <param name="filter">Filter criteria for reviews such as rating and campaign ID.</param>
        /// <returns>Returns a paginated list of reviews received.</returns>
        [HttpGet("my/reviews")]
        [Authorize(Roles = "Marketer")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ReviewDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyReviews([FromQuery] ReviewFilterDto? filter)
        {
            var marketerId = GetMarketerId();
            if (marketerId == 0)
                return Unauthorized();

            var result = await _servicesManager.ReviewService.GetReviewsAsync(marketerId, filter);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves reviews given by the authenticated marketer.
        /// </summary>
        /// <param name="page">Page number.</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <returns>Returns a paginated list of reviews given.</returns>
        [HttpGet("my/reviews/given")]
        [Authorize(Roles = "Marketer")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ReviewDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReviewsGiven([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var marketerId = GetMarketerId();
            if (marketerId == 0)
                return Unauthorized();

            var result = await _servicesManager.ReviewService.GetReviewsGivenAsync(marketerId, page, pageSize);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves the average rating for the authenticated marketer.
        /// </summary>
        /// <returns>Returns the average rating score.</returns>
        [HttpGet("my/average-rating")]
        [Authorize(Roles = "Marketer")]
        [ProducesResponseType(typeof(ApiResponse<AverageRatingDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAverageRating()
        {
            var marketerId = GetMarketerId();
            if (marketerId == 0)
                return Unauthorized();

            var result = await _servicesManager.ReviewService.GetAverageRatingAsync(marketerId);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        private int GetMarketerId()
        {
            var marketerId = User.FindFirst("marketerId")?.Value;
            return int.TryParse(marketerId, out var id) ? id : 0;
        }
    }
}
