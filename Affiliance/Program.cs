using Affiliance_Api.Extensions;
using Microsoft.AspNetCore.Identity;

namespace Affiliance
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers().AddJsonOptions(option =>
            {
                option.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                option.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSqlConnection(builder.Configuration);
            builder.Services.AddIdentityConfiguration();
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddSwaggerConfiguration();
            builder.Services.AddCorsConfiguration();
            builder.Services.AddServices();
            builder.Services.AddAutoMapperConfigration();

            var app = builder.Build();

            // 1. Exception handler FIRST
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";
                    var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
                    if (error != null)
                    {
                        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogError(error.Error, "Unhandled exception");
                        await context.Response.WriteAsJsonAsync(new
                        {
                            success = false,
                            message = $"Internal server error: {error.Error.Message}",
                            data = (object?)null
                        });
                    }
                });
            });

            // 2. Swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Affiliance API v1");
                c.RoutePrefix = "swagger";
            });

            // 3. CORS, Auth
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();

            // 4. Map endpoints
            app.MapGet("/", () => Results.Redirect("/swagger"))
                .ExcludeFromDescription();

            app.MapPost("/api/admin/reset-seed", async (
                UserManager<Affiliance_core.Entites.User> userManager,
                RoleManager<IdentityRole<int>> roleManager,
                Affiliance_Infrasturcture.Data.AffiliancesDBcontext dbContext) =>
            {
                try
                {
                    await Affiliance_Infrasturcture.Data.DataSeeder.ResetAndReseedAsync(userManager, roleManager, dbContext);
                    return Results.Ok(new { success = true, message = "Database reset and re-seeded successfully." });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { success = false, message = $"Reset failed: {ex.Message}" });
                }
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .ExcludeFromDescription();

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

                    // Only seed fake data in Development environment
                    if (app.Environment.IsDevelopment())
                    {
                        await Affiliance_Infrasturcture.Data.DataSeeder.SeedAllDataAsync(userManager, dbContext);
                    }
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
