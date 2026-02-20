using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Affiliance_core.Entites
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required, MaxLength(250)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

       
        public NotificationType Type { get; set; }

       
        public int? RelatedId { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

     

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}