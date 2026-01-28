using Affiliance_core.Entites;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Affiliance_core.Entites
{
    public class User : IdentityUser<int>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePicture { get; set; }
        public string? RefreshToken { get; set; }

        [Column("balance", TypeName = "decimal(18, 2)")]
        public decimal Balance { get; set; } = 0.00m;

        public UserStatus Status { get; set; } = UserStatus.Active;

        public DateTime? RefreshTokenExpiryTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; } 
        public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

        
        public virtual Marketer? Marketer { get; set; }
        public virtual Company? Company { get; set; }
        public virtual Admin? Admin { get; set; } 


        public virtual ICollection<Review> ReviewsGiven { get; set; } = new List<Review>();
        public virtual ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
        public virtual ICollection<Complaint> ComplaintsFiled { get; set; } = new List<Complaint>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }

    public enum UserStatus
    {
        Active,
        Inactive,
        Suspended,
        PendingVerification
    }
}