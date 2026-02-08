using System.ComponentModel.DataAnnotations;

namespace Affiliance_core.Dto.CategoryDto
{
    public class UpdateCategoryDto
    {
        [StringLength(150)]
        public string? NameEn { get; set; }

        [StringLength(150)]
        public string? NameAr { get; set; }

        [StringLength(150)]
        public string? Slug { get; set; }

        [StringLength(200)]
        public string? Icon { get; set; }

        public int? ParentId { get; set; }
    }
}
