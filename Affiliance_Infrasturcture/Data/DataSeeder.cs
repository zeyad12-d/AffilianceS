using Affiliance_core.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

        public static async Task SeedCategoriesAsync(AffiliancesDBcontext context)
        {
            if (await context.Categories.AnyAsync())
                return;

            var categories = new List<Category>
            {
                // Root Categories
                new Category { NameEn = "Technology", NameAr = "التكنولوجيا", Slug = "technology", Icon = "💻" },
                new Category { NameEn = "Fashion", NameAr = "الموضة", Slug = "fashion", Icon = "👗" },
                new Category { NameEn = "Health & Beauty", NameAr = "الصحة والجمال", Slug = "health-beauty", Icon = "💄" },
                new Category { NameEn = "Food & Beverage", NameAr = "الطعام والشراب", Slug = "food-beverage", Icon = "🍔" },
                new Category { NameEn = "Travel", NameAr = "السفر", Slug = "travel", Icon = "✈️" },
                new Category { NameEn = "Education", NameAr = "التعليم", Slug = "education", Icon = "📚" },
                new Category { NameEn = "Finance", NameAr = "المالية", Slug = "finance", Icon = "💰" },
                new Category { NameEn = "Entertainment", NameAr = "الترفيه", Slug = "entertainment", Icon = "🎬" }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();

            // Add subcategories
            var techCategory = categories.First(c => c.Slug == "technology");
            var fashionCategory = categories.First(c => c.Slug == "fashion");
            var healthCategory = categories.First(c => c.Slug == "health-beauty");

            var subcategories = new List<Category>
            {
                new Category { NameEn = "Software", NameAr = "البرمجيات", Slug = "software", Icon = "💿", ParentId = techCategory.Id },
                new Category { NameEn = "Gadgets", NameAr = "الأجهزة", Slug = "gadgets", Icon = "📱", ParentId = techCategory.Id },
                new Category { NameEn = "Men's Fashion", NameAr = "أزياء رجالية", Slug = "mens-fashion", Icon = "👔", ParentId = fashionCategory.Id },
                new Category { NameEn = "Women's Fashion", NameAr = "أزياء نسائية", Slug = "womens-fashion", Icon = "👗", ParentId = fashionCategory.Id },
                new Category { NameEn = "Skincare", NameAr = "العناية بالبشرة", Slug = "skincare", Icon = "🧴", ParentId = healthCategory.Id },
                new Category { NameEn = "Makeup", NameAr = "المكياج", Slug = "makeup", Icon = "💅", ParentId = healthCategory.Id }
            };

            context.Categories.AddRange(subcategories);
            await context.SaveChangesAsync();
        }

        public static async Task SeedCompaniesAsync(UserManager<User> userManager, AffiliancesDBcontext context)
        {
            if (await context.Companies.AnyAsync())
                return;

            var companies = new[]
            {
                new { Email = "techcorp@example.com", Name = "TechCorp Solutions", Password = "Company@123", Address = "123 Tech Street, Dubai, UAE", Phone = "+971501234567", Website = "https://techcorp.com", TaxId = "TAX123456" },
                new { Email = "fashionhub@example.com", Name = "Fashion Hub", Password = "Company@123", Address = "456 Fashion Avenue, Riyadh, KSA", Phone = "+966501234567", Website = "https://fashionhub.com", TaxId = "TAX789012" },
                new { Email = "healthplus@example.com", Name = "Health Plus", Password = "Company@123", Address = "789 Health Road, Cairo, Egypt", Phone = "+201234567890", Website = "https://healthplus.com", TaxId = "TAX345678" },
                new { Email = "foodie@example.com", Name = "Foodie Delights", Password = "Company@123", Address = "321 Food Street, Amman, Jordan", Phone = "+962791234567", Website = "https://foodie.com", TaxId = "TAX901234" },
                new { Email = "travelworld@example.com", Name = "Travel World", Password = "Company@123", Address = "654 Travel Boulevard, Beirut, Lebanon", Phone = "+9613123456", Website = "https://travelworld.com", TaxId = "TAX567890" }
            };

            foreach (var companyData in companies)
            {
                var existingUser = await userManager.FindByEmailAsync(companyData.Email);
                if (existingUser != null) continue;

                var user = new User
                {
                    UserName = companyData.Email,
                    Email = companyData.Email,
                    FirstName = companyData.Name.Split(' ')[0],
                    LastName = string.Join(" ", companyData.Name.Split(' ').Skip(1)),
                    EmailConfirmed = true,
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, companyData.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Company");

                    var company = new Company
                    {
                        UserId = user.Id,
                        CampanyName = companyData.Name,
                        Address = companyData.Address,
                        PhoneNumber = companyData.Phone,
                        Website = companyData.Website,
                        TaxId = companyData.TaxId,
                        ContactEmail = companyData.Email,
                        CommercialRegister = "/uploads/commercial-register.pdf",
                        LogoUrl = "/uploads/logo.png",
                        Description = $"Leading company in the industry",
                        IsVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    context.Companies.Add(company);
                }
            }

            await context.SaveChangesAsync();
        }

        public static async Task SeedMarketersAsync(UserManager<User> userManager, AffiliancesDBcontext context)
        {
            if (await context.Marketers.AnyAsync())
                return;

            var marketers = new[]
            {
                new { Email = "marketer1@example.com", FirstName = "Ahmed", LastName = "Ali", Password = "Marketer@123", Bio = "Experienced digital marketer with 5+ years in social media marketing", Niche = "Technology", Skills = "Social Media, Content Creation, SEO" },
                new { Email = "marketer2@example.com", FirstName = "Sara", LastName = "Mohammed", Password = "Marketer@123", Bio = "Fashion influencer and lifestyle blogger", Niche = "Fashion", Skills = "Fashion, Lifestyle, Photography" },
                new { Email = "marketer3@example.com", FirstName = "Omar", LastName = "Hassan", Password = "Marketer@123", Bio = "Tech reviewer and gadget enthusiast", Niche = "Technology", Skills = "Tech Reviews, Video Production, YouTube" },
                new { Email = "marketer4@example.com", FirstName = "Layla", LastName = "Ibrahim", Password = "Marketer@123", Bio = "Beauty and skincare expert", Niche = "Health & Beauty", Skills = "Beauty, Skincare, Makeup Tutorials" },
                new { Email = "marketer5@example.com", FirstName = "Khalid", LastName = "Saeed", Password = "Marketer@123", Bio = "Food blogger and restaurant reviewer", Niche = "Food & Beverage", Skills = "Food Photography, Restaurant Reviews, Recipe Creation" },
                new { Email = "marketer6@example.com", FirstName = "Fatima", LastName = "Ahmed", Password = "Marketer@123", Bio = "Travel content creator", Niche = "Travel", Skills = "Travel Vlogging, Photography, Adventure Content" },
                new { Email = "marketer7@example.com", FirstName = "Youssef", LastName = "Mahmoud", Password = "Marketer@123", Bio = "Fitness and wellness coach", Niche = "Health & Beauty", Skills = "Fitness, Wellness, Nutrition" },
                new { Email = "marketer8@example.com", FirstName = "Nour", LastName = "Salem", Password = "Marketer@123", Bio = "Gaming content creator and streamer", Niche = "Entertainment", Skills = "Gaming, Streaming, Esports" }
            };

            foreach (var marketerData in marketers)
            {
                var existingUser = await userManager.FindByEmailAsync(marketerData.Email);
                if (existingUser != null) continue;

                var user = new User
                {
                    UserName = marketerData.Email,
                    Email = marketerData.Email,
                    FirstName = marketerData.FirstName,
                    LastName = marketerData.LastName,
                    EmailConfirmed = true,
                    Status = UserStatus.Active,
                    Balance = new Random().Next(100, 5000),
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, marketerData.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Marketer");

                    var marketer = new Marketer
                    {
                        UserId = user.Id,
                        Bio = marketerData.Bio,
                        Niche = marketerData.Niche,
                        SkillsExtracted = marketerData.Skills,
                        CvPath = "/uploads/cv.pdf",
                        NationalIdPath = "/uploads/national-id.pdf",
                        SocialLinks = "https://instagram.com/" + marketerData.FirstName.ToLower(),
                        TotalEarnings = new Random().Next(500, 10000),
                        PerformanceScore = (decimal)(new Random().NextDouble() * 2 + 3), // 3.0 to 5.0
                        PersonalityScore = new Random().Next(60, 100),
                        PersonalityTestTaken = true,
                        IsVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    context.Marketers.Add(marketer);
                }
            }

            await context.SaveChangesAsync();
        }

        public static async Task SeedCampaignsAsync(AffiliancesDBcontext context)
        {
            if (await context.Campaigns.AnyAsync())
                return;

            var companies = await context.Companies.ToListAsync();
            var categories = await context.Categories.Where(c => c.ParentId == null).ToListAsync();
            var admin = await context.Admins.FirstOrDefaultAsync();

            if (!companies.Any() || !categories.Any())
                return;

            var campaigns = new List<Campaign>();

            foreach (var company in companies.Take(3))
            {
                var category = categories[new Random().Next(categories.Count)];
                var startDate = DateTime.UtcNow.AddDays(-30);
                var endDate = DateTime.UtcNow.AddDays(60);

                campaigns.Add(new Campaign
                {
                    CompanyId = company.Id,
                    CategoryId = category.Id,
                    Title = $"{company.CampanyName} - {category.NameEn} Campaign",
                    Description = $"Join our exciting {category.NameEn.ToLower()} campaign and earn great commissions!",
                    CommissionType = CommissionType.Percentage,
                    CommissionValue = new Random().Next(5, 25),
                    Budget = new Random().Next(5000, 50000),
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = CampaignStatus.Active,
                    TrackingBaseUrl = $"https://track.{company.CampanyName.Replace(" ", "").ToLower()}.com",
                    PromotionalMaterials = "/uploads/promo-materials.zip",
                    ApprovedBy = admin?.UserId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Add some pending campaigns
            if (companies.Count > 3)
            {
                var category = categories[new Random().Next(categories.Count)];
                campaigns.Add(new Campaign
                {
                    CompanyId = companies[3].Id,
                    CategoryId = category.Id,
                    Title = $"{companies[3].CampanyName} - New Campaign",
                    Description = "A new campaign waiting for approval",
                    CommissionType = CommissionType.Fixed,
                    CommissionValue = 100,
                    Budget = 10000,
                    StartDate = DateTime.UtcNow.AddDays(7),
                    EndDate = DateTime.UtcNow.AddDays(90),
                    Status = CampaignStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                });
            }

            context.Campaigns.AddRange(campaigns);
            await context.SaveChangesAsync();
        }

        public static async Task SeedCampaignApplicationsAsync(AffiliancesDBcontext context)
        {
            if (await context.CampaignApplications.AnyAsync())
                return;

            var campaigns = await context.Campaigns.Where(c => c.Status == CampaignStatus.Active).ToListAsync();
            var marketers = await context.Marketers.ToListAsync();

            if (!campaigns.Any() || !marketers.Any())
                return;

            var applications = new List<CampaignApplication>();
            var random = new Random();

            foreach (var campaign in campaigns)
            {
                var selectedMarketers = marketers.OrderBy(x => random.Next()).Take(random.Next(2, 5));

                foreach (var marketer in selectedMarketers)
                {
                    var status = (ApplicationStatus)random.Next(0, 3); // Pending, Accepted, Rejected
                    applications.Add(new CampaignApplication
                    {
                        CampaignId = campaign.Id,
                        MarketerId = marketer.Id,
                        Status = status,
                        AiMatchScore = (decimal)(random.NextDouble() * 0.5 + 0.5), // 0.5 to 1.0
                        AppliedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                        RespondedAt = status != ApplicationStatus.Pending ? DateTime.UtcNow.AddDays(-random.Next(1, 15)) : null,
                        ResponseNote = status != ApplicationStatus.Pending 
                            ? (status == ApplicationStatus.Accepted ? "Welcome to the campaign!" : "Thank you for your interest.")
                            : null
                    });
                }
            }

            context.CampaignApplications.AddRange(applications);
            await context.SaveChangesAsync();
        }

        public static async Task SeedReviewsAsync(AffiliancesDBcontext context)
        {
            if (await context.Reviews.AnyAsync())
                return;

            var companies = await context.Companies.ToListAsync();
            var marketers = await context.Marketers.ToListAsync();
            var campaigns = await context.Campaigns.Take(3).ToListAsync();

            if (!companies.Any() || !marketers.Any())
                return;

            var reviews = new List<Review>();
            var random = new Random();

            // Marketers reviewing companies
            foreach (var company in companies.Take(2))
            {
                var reviewer = marketers[random.Next(marketers.Count)];
                var campaign = campaigns.FirstOrDefault();

                reviews.Add(new Review
                {
                    ReviewerId = reviewer.UserId,
                    ReviewedId = company.UserId,
                    CampaignId = campaign?.Id,
                    Rating = (byte)random.Next(4, 6), // 4 or 5
                    Comment = $"Great company to work with! Excellent communication and timely payments.",
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 60))
                });
            }

            // Companies reviewing marketers
            foreach (var marketer in marketers.Take(3))
            {
                var reviewer = companies[random.Next(companies.Count)];
                var campaign = campaigns.FirstOrDefault();

                reviews.Add(new Review
                {
                    ReviewerId = reviewer.UserId,
                    ReviewedId = marketer.UserId,
                    CampaignId = campaign?.Id,
                    Rating = (byte)random.Next(4, 6),
                    Comment = $"Outstanding performance! Highly recommended marketer.",
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 60))
                });
            }

            context.Reviews.AddRange(reviews);
            await context.SaveChangesAsync();
        }

        public static async Task SeedAllDataAsync(UserManager<User> userManager, AffiliancesDBcontext context)
        {
            await SeedCategoriesAsync(context);
            await SeedCompaniesAsync(userManager, context);
            await SeedMarketersAsync(userManager, context);
            await SeedCampaignsAsync(context);
            await SeedCampaignApplicationsAsync(context);
            await SeedReviewsAsync(context);
        }
    }
}
