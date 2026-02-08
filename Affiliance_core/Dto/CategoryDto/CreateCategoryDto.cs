using System.ComponentModel.DataAnnotations;

namespace Affiliance_core.Dto.CategoryDto
{
    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "English Name is required")]
        [StringLength(150)]
        public string NameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Name is required")]
        [StringLength(150)]
        public string NameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug is required")]
        [StringLength(150)]
        public string Slug { get; set; } = string.Empty;

        [StringLength(200)]
        public string Icon { get; set; } = string.Empty;

        public int? ParentId { get; set; }
    }
}
