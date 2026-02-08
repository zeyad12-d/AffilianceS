namespace Affiliance_core.Dto.CategoryDto
{
    public class CategoryDetailsDto : CategoryDto
    {
        public List<CategoryDto> Children { get; set; } = new List<CategoryDto>();
        public CategoryDto? Parent { get; set; }
    }
}
