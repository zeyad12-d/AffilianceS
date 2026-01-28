using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Affiliance_core.Entites
{
    public class User:IdentityUser<int>
    {
        public string? FirstName { get; set; } 
        public string? LastName { get; set; } 
        public string? ProfilePicture { get; set; }
        public string? RefreshToken { get; set; }

        [Column("balance", TypeName = "decimal(18, 2)")]
        public decimal Balance { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime DeletedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
        public DateTime LastLogoutAt { get; set; } = DateTime.UtcNow;
        public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

        // RELATIONSHIPS
        public virtual Marketer? Marketer { get; set; }
        public virtual Company? Company { get; set; }
    }
}
