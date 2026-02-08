using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.CategoryDto;
using Affiliance_core.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Affiliance_Api.Controllers
{
    /// <summary>
    /// Controller for managing categories including CRUD operations and hierarchy management.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IServicesManager _servicesManager;

        public CategoryController(IServicesManager servicesManager)
        {
            _servicesManager = servicesManager;
        }

        #region GET Endpoints (Public/Authorized)

        /// <summary>
        /// Get all categories
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCategories()
        {
            var result = await _servicesManager.CategoryService.GetAllCategoriesAsync();
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Get root categories (categories without parent)
        /// </summary>
        [HttpGet("roots")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRootCategories()
        {
            var result = await _servicesManager.CategoryService.GetRootCategoriesAsync();
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Get category by ID with full details
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CategoryDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var result = await _servicesManager.CategoryService.GetCategoryByIdAsync(id);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        /// <summary>
        /// Get children of a category
        /// </summary>
        [HttpGet("{id}/children")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategoryChildren(int id)
        {
            var result = await _servicesManager.CategoryService.GetCategoryChildrenAsync(id);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Get category hierarchy (tree structure)
        /// </summary>
        [HttpGet("hierarchy")]
        [ProducesResponseType(typeof(ApiResponse<CategoryTreeDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategoryHierarchy()
        {
            var result = await _servicesManager.CategoryService.GetCategoryHierarchyAsync();
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Get category by slug
        /// </summary>
        [HttpGet("slug/{slug}")]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategoryBySlug(string slug)
        {
            var result = await _servicesManager.CategoryService.GetCategoryBySlugAsync(slug);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        /// <summary>
        /// Get campaigns for a category (paged)
        /// </summary>
        [HttpGet("{id}/campaigns")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategoryCampaigns(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _servicesManager.CategoryService.GetCategoryCampaignsAsync(id, page, pageSize);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        #endregion

        #region POST Endpoints (Admin Only)

        /// <summary>
        /// Create a new category
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _servicesManager.CategoryService.CreateCategoryAsync(dto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Create multiple categories at once
        /// </summary>
        [HttpPost("bulk")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCategoriesBulk([FromBody] List<CreateCategoryDto> dtos)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _servicesManager.CategoryService.CreateCategoriesBulkAsync(dtos);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        #endregion

        #region PUT Endpoints (Admin Only)

        /// <summary>
        /// Update a category
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _servicesManager.CategoryService.UpdateCategoryAsync(id, dto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        #endregion

        #region DELETE Endpoints (Admin Only)

        /// <summary>
        /// Delete a category
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _servicesManager.CategoryService.DeleteCategoryAsync(id);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Delete a category safely (checks for campaigns)
        /// </summary>
        [HttpDelete("{id}/safe")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategorySafe(int id)
        {
            var result = await _servicesManager.CategoryService.DeleteCategorySafeAsync(id);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        #endregion
    }
}
