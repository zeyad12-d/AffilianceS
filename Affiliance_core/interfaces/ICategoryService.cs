using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.CategoryDto;

namespace Affiliance_core.interfaces
{
    public interface ICategoryService
    {
        // GET Methods
        Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllCategoriesAsync();
        Task<ApiResponse<IEnumerable<CategoryDto>>> GetRootCategoriesAsync();
        Task<ApiResponse<CategoryDetailsDto>> GetCategoryByIdAsync(int id);
        Task<ApiResponse<IEnumerable<CategoryDto>>> GetCategoryChildrenAsync(int parentId);
        Task<ApiResponse<CategoryTreeDto>> GetCategoryHierarchyAsync();
        Task<ApiResponse<CategoryDto>> GetCategoryBySlugAsync(string slug);
        Task<ApiResponse<PagedResult<CategoryDto>>> GetCategoryCampaignsAsync(int categoryId, int page = 1, int pageSize = 10);
        
        // POST Methods (Admin Only)
        Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryDto dto);
        Task<ApiResponse<IEnumerable<CategoryDto>>> CreateCategoriesBulkAsync(List<CreateCategoryDto> dtos);
        
        // PUT Methods (Admin Only)
        Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(int id, UpdateCategoryDto dto);
        
        // DELETE Methods (Admin Only)
        Task<ApiResponse<bool>> DeleteCategoryAsync(int id);
        Task<ApiResponse<bool>> DeleteCategorySafeAsync(int id);
    }
}
