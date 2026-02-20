using Affiliance_core.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Affiliance_Infrasturcture.Data
{
    // Fix: Specify the correct key type for IdentityDbContext<User, IdentityRole<int>, int>
    public class AffiliancesDBcontext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public AffiliancesDBcontext(DbContextOptions<AffiliancesDBcontext> options) : base(options)
        {
        }

        // البروفايلات الأساسية
        public DbSet<Marketer> Marketers { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Admin> Admins { get; set; }

        // العمليات والبيزنس
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<CampaignApplication> CampaignApplications { get; set; }
        public DbSet<TrackingLink> TrackingLinks { get; set; }
        public DbSet<PerformanceLog> PerformanceLogs { get; set; }

        // الملحقات
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AiSuggestion> AiSuggestions { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // مهم جداً لضبط جداول الـ Identity

            // --- 1. One-to-One Relationships (User Profiles) ---

            builder.Entity<User>()
                .HasOne(u => u.Marketer)
                .WithOne(m => m.User)
                .HasForeignKey<Marketer>(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<User>()
                .HasOne(u => u.Company)
                .WithOne(c => c.User)
                .HasForeignKey<Company>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<User>()
                .HasOne(u => u.Admin)
                .WithOne(a => a.User)
                .HasForeignKey<Admin>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

              builder.Entity<CampaignApplication>()
           .HasOne(ca => ca.Marketer)
           .WithMany(m => m.CampaignApplications)
            .HasForeignKey(ca => ca.MarketerId)
           .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<CampaignApplication>()
                .HasOne(ca => ca.Campaign)
                .WithMany(c => c.CampaignApplications)
                .HasForeignKey(ca => ca.CampaignId)
                .OnDelete(DeleteBehavior.NoAction);

            // TrackingLink relationships - use NoAction to prevent cascade cycles
            builder.Entity<TrackingLink>()
                .HasOne(tl => tl.Campaign)
                .WithMany(c => c.TrackingLinks)
                .HasForeignKey(tl => tl.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TrackingLink>()
                .HasOne(tl => tl.Marketer)
                .WithMany()
                .HasForeignKey(tl => tl.MarketerId)
                .OnDelete(DeleteBehavior.NoAction);


            var decimalEntities = builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?));

            foreach (var property in decimalEntities)
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }

      
            builder.Entity<AiSuggestion>()
                .Property(s => s.Score)
                .HasPrecision(5, 3);

            builder.Entity<CampaignApplication>()
                .Property(a => a.AiMatchScore)
                .HasPrecision(5, 3);

            

            builder.Entity<Review>(entity =>
            {
                entity.HasOne(r => r.Reviewer).WithMany(u => u.ReviewsGiven).HasForeignKey(r => r.ReviewerId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(r => r.Reviewed).WithMany(u => u.ReviewsReceived).HasForeignKey(r => r.ReviewedId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Complaint>(entity =>
            {
                entity.HasOne(c => c.Complainant).WithMany(u => u.ComplaintsFiled).HasForeignKey(c => c.ComplainantId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(c => c.Defendant).WithMany().HasForeignKey(c => c.DefendantId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(c => c.ResolvedByNavigation).WithMany().HasForeignKey(c => c.ResolvedBy).OnDelete(DeleteBehavior.SetNull);
            });

         
            builder.Entity<TrackingLink>()
                .HasIndex(t => t.UniqueLink)
                .IsUnique();
        }
    }
}