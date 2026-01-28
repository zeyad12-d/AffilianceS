using Affiliance_core.Entites;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Affiliance_core.Entites
{
    public class Company
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage ="Campany Name Is Required")]
        public string CampanyName { get; set; } =string.Empty;

        [Required(ErrorMessage = "Address Is Required"),MinLength(10)]
        public string Address { get; set; }= string.Empty;
        [Required(ErrorMessage = "Phone Number Is Required"),MinLength(10),MaxLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required(ErrorMessage = "Website Is Required")]
        public string Website { get; set; }= string.Empty;

        [Required(ErrorMessage = "Commercial Register Is Required"),MinLength(20),MaxLength(200)]
        public string CommercialRegister { get; set; } = string.Empty;
        public string? TaxId { get; set; }
        public string? LogoUrl { get; set; }
        public string? Description { get; set; }
        public string? ContactEmail { get; set; }= string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // RELATIONSHIPS
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; } 
    }
}
