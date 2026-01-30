using Affiliance_core.Entites;
using Microsoft.AspNetCore.Identity;

namespace Affiliance_Infrasturcture.Data
{
    public class DataSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole<int>> roleManager)
        {
            var roles = new List<string> { "User", "Marketer", "Company", "Admin" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
            }
        }

        public static async Task SeedAdminAsync(UserManager<User> userManager, AffiliancesDBcontext context)
        {
            var adminEmail = "admin@affiliance.com";
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin == null)
            {
                var user = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Admin",
                    EmailConfirmed = true,
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");

                    var adminEntity = new Admin
                    {
                        UserId = user.Id,
                        Department = AdminDepartment.SuperAdmin,
                        CanManageUsers = true,
                        CanManageCampaigns = true,
                        CanManagePayments = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Admins.Add(adminEntity);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
