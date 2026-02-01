using Affiliance_Api.Extensions;
using Microsoft.AspNetCore.Identity; // Add this using directive

namespace Affiliance
{
    public class Program
    {
        public static async Task Main(string[] args) // Change to async Task
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSqlConnection(builder.Configuration);
            builder.Services.AddIdentityConfiguration();
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddSwaggerConfiguration();
            builder.Services.AddCorsConfiguration();
            builder.Services.AddServices();
            builder.Services.AddAutoMapperConfigration();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            // Seed Data
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
                    await Affiliance_Infrasturcture.Data.DataSeeder.SeedRolesAsync(roleManager);

                    var userManager = services.GetRequiredService<UserManager<Affiliance_core.Entites.User>>();
                    var dbContext = services.GetRequiredService<Affiliance_Infrasturcture.Data.AffiliancesDBcontext>();
                    await Affiliance_Infrasturcture.Data.DataSeeder.SeedAdminAsync(userManager, dbContext);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            app.Run();
        }
    }
}
