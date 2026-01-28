using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Affiliance_core.Entities
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        
        public AdminDepartment Department { get; set; }

        public bool CanManageUsers { get; set; } = true;
        public bool CanManageCampaigns { get; set; } = true;
        public bool CanManagePayments { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

       
        public virtual ICollection<Complaint> ResolvedComplaints { get; set; } = new List<Complaint>();
        public virtual ICollection<Campaign> ApprovedCampaigns { get; set; } = new List<Campaign>();
    }

    public enum AdminDepartment
    {
        SuperAdmin,    
        Support,       
        Content,      
        Finance,       
        Compliance     
    }
}