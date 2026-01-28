using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Affiliance_core.Entites
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Category Name Is Required")]
        [Column("name_ar")]
        [StringLength(150)]
        public string NameAr { get; set; } = string.Empty;

        [Required]
        [Column("name_en")]
        [StringLength(150)]
        public string NameEn { get; set; } = string.Empty;

        [Required]
        [Column("slug")]
        [StringLength(150)]
        public string Slug { get; set; } = string.Empty;

        [Column("icon")]
        [StringLength(200)]
        public string Icon { get; set; } = string.Empty;

        // Nullable to allow root categories
        [Column("parent_id")]
        public int? ParentId { get; set; }

        // Navigation to parent category
        [ForeignKey("ParentId")]
        public Category? Parent { get; set; }

        // Navigation to child categories
        public ICollection<Category> Children { get; set; } = new List<Category>();
        public  ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
    }
}
