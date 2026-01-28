using Affiliance_core.Entites;
using Affiliance_core.interfaces;
using Affiliance_Infrasturcture.Data;
using Affiliance_Infrasturcture.Repostiory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Affiliance_Api.Extensions
{
    public static class Extensions
    {
        #region AddSql
        public static void AddSqlConnection(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AffiliancesDBcontext>(options =>
                options.UseSqlServer(config.GetConnectionString("cs")));
        }
        #endregion

        #region Identity
        public static void AddIdentityConfiguration(this IServiceCollection services)
        {
            // لاحظ استخدام <User, IdentityRole<int>> عشان الـ IDs
            services.AddIdentity<User, IdentityRole<int>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AffiliancesDBcontext>()
            .AddDefaultTokenProviders();
        }
        #endregion

        #region Swagger
        public static void AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Affiliance API",
                    Version = "v1",
                    Description = "API documentation for the Affiliate Platform project."
                });

            
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your valid JWT token.",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });
        }
        #endregion

        #region JWT
        public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
        {
            var jwtSettings = config.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });
        }
        #endregion

        #region Cors
        public static void AddCorsConfiguration(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
        }
        #endregion
        #region Services
        public static void AddServices(this IServiceCollection services)
        {
           
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IServicesManager, ServicesManager>();
            services.AddScoped<IServiceFactory, ServiceFactory>();
        }
        #endregion
    }
}