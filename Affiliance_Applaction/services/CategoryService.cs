using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.CategoryDto;
using Affiliance_core.Entites;
using Affiliance_core.interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Affiliance_Applaction.services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        #region GET Methods

        public async Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllCategoriesAsync()
        {
            var categories = await _unitOfWork.Repository<Category>()
                .FindAsync(c => true, new[] { "Parent", "Children", "Campaigns" });

            var categoriesDto = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            return ApiResponse<IEnumerable<CategoryDto>>.CreateSuccess(categoriesDto, "Categories retrieved successfully");
        }

        public async Task<ApiResponse<IEnumerable<CategoryDto>>> GetRootCategoriesAsync()
        {
            var rootCategories = await _unitOfWork.Repository<Category>()
                .FindAsync(c => c.ParentId == null, new[] { "Children", "Campaigns" });

            var categoriesDto = _mapper.Map<IEnumerable<CategoryDto>>(rootCategories);
            return ApiResponse<IEnumerable<CategoryDto>>.CreateSuccess(categoriesDto, "Root categories retrieved successfully");
        }

        public async Task<ApiResponse<CategoryDetailsDto>> GetCategoryByIdAsync(int id)
        {
            // Load related data
            var categoryWithRelations = await _unitOfWork.Repository<Category>()
                .FindAsync(c => c.Id == id, new[] { "Parent", "Children", "Campaigns" });

            var categoryEntity = categoryWithRelations.FirstOrDefault();
            if (categoryEntity == null)
                return ApiResponse<CategoryDetailsDto>.CreateFail("Category not found");

            var categoryDto = _mapper.Map<CategoryDetailsDto>(categoryEntity);
            
            // Map children
            if (categoryEntity.Children != null && categoryEntity.Children.Any())
            {
                categoryDto.Children = _mapper.Map<List<CategoryDto>>(categoryEntity.Children);
            }
            else
            {
                categoryDto.Children = new List<CategoryDto>();
            }

            // Map parent
            if (categoryEntity.Parent != null)
            {
                categoryDto.Parent = _mapper.Map<CategoryDto>(categoryEntity.Parent);
            }

            return ApiResponse<CategoryDetailsDto>.CreateSuccess(categoryDto, "Category retrieved successfully");
        }

        public async Task<ApiResponse<IEnumerable<CategoryDto>>> GetCategoryChildrenAsync(int parentId)
        {
            var parent = await _unitOfWork.Repository<Category>().GetByIdAsync(parentId);
            if (parent == null)
                return ApiResponse<IEnumerable<CategoryDto>>.CreateFail("Parent category not found");

            var children = await _unitOfWork.Repository<Category>()
                .FindAsync(c => c.ParentId == parentId, new[] { "Children", "Campaigns" });

            var childrenDto = _mapper.Map<IEnumerable<CategoryDto>>(children);
            return ApiResponse<IEnumerable<CategoryDto>>.CreateSuccess(childrenDto, "Category children retrieved successfully");
        }

        public async Task<ApiResponse<CategoryTreeDto>> GetCategoryHierarchyAsync()
        {
            try
            {
                var rootCategories = await _unitOfWork.Repository<Category>()
                    .FindAsync(c => c.ParentId == null, new[] { "Children", "Children.Children", "Campaigns" });

                var treeDto = new CategoryTreeDto
                {
                    RootCategories = BuildCategoryTree(rootCategories).ToList()
                };

                return ApiResponse<CategoryTreeDto>.CreateSuccess(treeDto, "Category hierarchy retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CategoryTreeDto>.CreateFail($"Error building category hierarchy: {ex.Message}");
            }
        }

        private IEnumerable<CategoryTreeNodeDto> BuildCategoryTree(IEnumerable<Category> categories)
        {
            if (categories == null)
                yield break;

            foreach (var category in categories)
            {
                var node = _mapper.Map<CategoryTreeNodeDto>(category);
                
                if (category.Children != null && category.Children.Any())
                {
                    node.Children = BuildCategoryTree(category.Children).ToList();
                }
                else
                {
                    node.Children = new List<CategoryTreeNodeDto>();
                }
                
                yield return node;
            }
        }

        public async Task<ApiResponse<CategoryDto>> GetCategoryBySlugAsync(string slug)
        {
            var categories = await _unitOfWork.Repository<Category>()
                .FindAsync(c => c.Slug == slug, new[] { "Parent", "Children", "Campaigns" });

            var category = categories.FirstOrDefault();
            if (category == null)
                return ApiResponse<CategoryDto>.CreateFail("Category not found");

            var categoryDto = _mapper.Map<CategoryDto>(category);
            return ApiResponse<CategoryDto>.CreateSuccess(categoryDto, "Category retrieved successfully");
        }

        public async Task<ApiResponse<PagedResult<CategoryDto>>> GetCategoryCampaignsAsync(int categoryId, int page = 1, int pageSize = 10)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(categoryId);
            if (category == null)
                return ApiResponse<PagedResult<CategoryDto>>.CreateFail("Category not found");

            // This method should return campaigns, but for now we'll return empty as campaigns DTOs are not created yet
            // TODO: Implement when CampaignDto is available
            var result = new PagedResult<CategoryDto>(
                data: new List<CategoryDto>(),
                page: page,
                pagesize: pageSize,
                totalCount: 0
            );

            return ApiResponse<PagedResult<CategoryDto>>.CreateSuccess(result, "Category campaigns retrieved successfully");
        }

        #endregion

        #region POST Methods

        public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryDto dto)
        {
            // Check if slug already exists
            var existingCategory = await _unitOfWork.Repository<Category>()
                .FindAsync(c => c.Slug == dto.Slug);

            if (existingCategory.Any())
                return ApiResponse<CategoryDto>.CreateFail("Category with this slug already exists");

            // Validate parent if provided
            if (dto.ParentId.HasValue)
            {
                var parent = await _unitOfWork.Repository<Category>().GetByIdAsync(dto.ParentId.Value);
                if (parent == null)
                    return ApiResponse<CategoryDto>.CreateFail("Parent category not found");
            }

            var category = _mapper.Map<Category>(dto);
            await _unitOfWork.Repository<Category>().AddAsync(category);
            await _unitOfWork.CompleteAsync();

            // Reload with relations
            var createdCategory = await _unitOfWork.Repository<Category>()
                .FindAsync(c => c.Id == category.Id, new[] { "Parent", "Children", "Campaigns" });

            var categoryDto = _mapper.Map<CategoryDto>(createdCategory.FirstOrDefault());
            return ApiResponse<CategoryDto>.CreateSuccess(categoryDto, "Category created successfully");
        }

        public async Task<ApiResponse<IEnumerable<CategoryDto>>> CreateCategoriesBulkAsync(List<CreateCategoryDto> dtos)
        {
            if (dtos == null || !dtos.Any())
                return ApiResponse<IEnumerable<CategoryDto>>.CreateFail("No categories provided");

            var createdCategories = new List<CategoryDto>();
            var errors = new List<string>();

            foreach (var dto in dtos)
            {
                try
                {
                    var slugExists = (await _unitOfWork.Repository<Category>().FindAsync(c => c.Slug == dto.Slug)).Any();
                    if (slugExists)
                    {
                        errors.Add($"Slug '{dto.Slug}' already exists");
                        continue;
                    }

                    var result = await CreateCategoryAsync(dto);
                    if (result.Success && result.Data != null)
                    {
                        createdCategories.Add(result.Data);
                    }
                    else
                    {
                        errors.Add($"Failed to create category {dto.NameEn}: {result.Message}");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Failed to create category {dto.NameEn}: {ex.Message}");
                }
            }

            if (createdCategories.Count == 0)
            {
                return ApiResponse<IEnumerable<CategoryDto>>.CreateFail(
                    $"All categories failed to create. Errors: {string.Join(", ", errors)}");
            }

            if (errors.Any())
            {
                return ApiResponse<IEnumerable<CategoryDto>>.CreateSuccess(
                    createdCategories,
                    $"{createdCategories.Count} created, {errors.Count} failed: {string.Join(", ", errors)}");
            }

            return ApiResponse<IEnumerable<CategoryDto>>.CreateSuccess(
                createdCategories, 
                $"{createdCategories.Count} categories created successfully");
        }

        #endregion

        #region PUT Methods

        public async Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
        {
            try
            {
                if (dto == null)
                    return ApiResponse<CategoryDto>.CreateFail("Update data is required");

                var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
                if (category == null)
                    return ApiResponse<CategoryDto>.CreateFail("Category not found");

                // Check if slug already exists (if being updated)
                if (!string.IsNullOrEmpty(dto.Slug) && dto.Slug != category.Slug)
                {
                    var existingCategory = await _unitOfWork.Repository<Category>()
                        .FindAsync(c => c.Slug == dto.Slug && c.Id != id);

                    if (existingCategory.Any())
                        return ApiResponse<CategoryDto>.CreateFail("Category with this slug already exists");
                }

                // Validate parent if provided
                if (dto.ParentId.HasValue && dto.ParentId.Value != category.ParentId)
                {
                    // Prevent circular reference
                    if (dto.ParentId.Value == id)
                        return ApiResponse<CategoryDto>.CreateFail("Category cannot be its own parent");

                    // Check if the new parent is a descendant of the current category (Correct Logic)
                    var isCycle = await IsAncestorOfAsync(id, dto.ParentId.Value);
                    if (isCycle)
                        return ApiResponse<CategoryDto>.CreateFail("Cannot set parent to one of its own descendants (circular reference)");

                    var parent = await _unitOfWork.Repository<Category>().GetByIdAsync(dto.ParentId.Value);
                    if (parent == null)
                        return ApiResponse<CategoryDto>.CreateFail("Parent category not found");
                        
                    category.ParentId = dto.ParentId.Value;
                }

                // Update only provided fields
                if (!string.IsNullOrEmpty(dto.NameEn)) category.NameEn = dto.NameEn;
                if (!string.IsNullOrEmpty(dto.NameAr)) category.NameAr = dto.NameAr;
                if (!string.IsNullOrEmpty(dto.Slug)) category.Slug = dto.Slug;
                if (!string.IsNullOrEmpty(dto.Icon)) category.Icon = dto.Icon;

                _unitOfWork.Repository<Category>().Update(category);
                await _unitOfWork.CompleteAsync();

                // Reload with relations
                var updatedCategory = await _unitOfWork.Repository<Category>()
                    .FindAsync(c => c.Id == category.Id, new[] { "Parent", "Children", "Campaigns" });

                var categoryEntity = updatedCategory.FirstOrDefault();
                if (categoryEntity == null)
                    return ApiResponse<CategoryDto>.CreateFail("Failed to reload updated category");

                var categoryDto = _mapper.Map<CategoryDto>(categoryEntity);
                return ApiResponse<CategoryDto>.CreateSuccess(categoryDto, "Category updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CategoryDto>.CreateFail($"Error updating category: {ex.Message}");
            }
        }

        private async Task<bool> IsAncestorOfAsync(int potentialAncestorId, int targetNodeId)
        {
            var currentId = targetNodeId;
            while(true)
            {
                var node = await _unitOfWork.Repository<Category>().GetByIdAsync(currentId);
                if (node == null || node.ParentId == null) return false;
                
                if (node.ParentId == potentialAncestorId) return true;
                
                currentId = node.ParentId.Value;
                if (currentId == targetNodeId) break;
            }
            return false;
        }

        #endregion

        #region DELETE Methods

        public async Task<ApiResponse<bool>> DeleteCategoryAsync(int id)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            if (category == null)
                return ApiResponse<bool>.CreateFail("Category not found");

            // Check if category has children
            var children = await _unitOfWork.Repository<Category>()
                .FindAsync(c => c.ParentId == id);

            if (children.Any())
                return ApiResponse<bool>.CreateFail("Cannot delete category with children. Please delete or move children first.");

            _unitOfWork.Repository<Category>().Delete(category);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, "Category deleted successfully");
        }

        public async Task<ApiResponse<bool>> DeleteCategorySafeAsync(int id)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            if (category == null)
                return ApiResponse<bool>.CreateFail("Category not found");

            // Check if category has children
            var children = await _unitOfWork.Repository<Category>()
                .FindAsync(c => c.ParentId == id);

            if (children.Any())
                return ApiResponse<bool>.CreateFail("Cannot delete category with children. Please delete or move children first.");

            // Check if category has campaigns
            var campaigns = await _unitOfWork.Repository<Campaign>()
                .FindAsync(c => c.CategoryId == id);

            if (campaigns.Any())
                return ApiResponse<bool>.CreateFail($"Cannot delete category. It has {campaigns.Count()} associated campaign(s).");

            _unitOfWork.Repository<Category>().Delete(category);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.CreateSuccess(true, "Category deleted successfully");
        }

        #endregion
    }
}
