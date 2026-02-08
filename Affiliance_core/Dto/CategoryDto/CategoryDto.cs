using System.ComponentModel.DataAnnotations;

namespace Affiliance_core.Dto.CategoryDto
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
        public int ChildrenCount { get; set; }
        public int CampaignsCount { get; set; }
    }
}
