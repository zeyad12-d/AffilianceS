namespace Affiliance_core.Dto.CategoryDto
{
    public class CategoryTreeDto
    {
        public List<CategoryTreeNodeDto> RootCategories { get; set; } = new List<CategoryTreeNodeDto>();
    }

    public class CategoryTreeNodeDto : CategoryDto
    {
        public List<CategoryTreeNodeDto> Children { get; set; } = new List<CategoryTreeNodeDto>();
    }
}
