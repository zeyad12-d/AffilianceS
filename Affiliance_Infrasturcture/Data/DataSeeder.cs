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
            var companies = new[]
            {
                new { Email = "techcorp@example.com", Name = "TechCorp Solutions", Password = "Company@123", Address = "123 Tech Street, Dubai, UAE", Phone = "+971501234567", Website = "https://techcorp.com", TaxId = "TAX123456" },
                new { Email = "fashionhub@example.com", Name = "Fashion Hub", Password = "Company@123", Address = "456 Fashion Avenue, Riyadh, KSA", Phone = "+966501234567", Website = "https://fashionhub.com", TaxId = "TAX789012" },
                new { Email = "healthplus@example.com", Name = "Health Plus", Password = "Company@123", Address = "789 Health Road, Cairo, Egypt", Phone = "+201234567890", Website = "https://healthplus.com", TaxId = "TAX345678" },
                new { Email = "foodie@example.com", Name = "Foodie Delights", Password = "Company@123", Address = "321 Food Street, Amman, Jordan", Phone = "+962791234567", Website = "https://foodie.com", TaxId = "TAX901234" },
                new { Email = "travelworld@example.com", Name = "Travel World", Password = "Company@123", Address = "654 Travel Boulevard, Beirut, Lebanon", Phone = "+9613123456", Website = "https://travelworld.com", TaxId = "TAX567890" }
            };

            var hasNewCompanies = false;

            foreach (var companyData in companies)
            {
                var existingUser = await userManager.FindByEmailAsync(companyData.Email);

                if (existingUser != null)
                {
                    // User exists - ensure the Company record also exists
                    var companyExists = await context.Companies.AnyAsync(c => c.UserId == existingUser.Id);
                    if (!companyExists)
                    {
                        // User exists but Company record is missing - recreate it
                        var company = new Company
                        {
                            UserId = existingUser.Id,
                            CampanyName = companyData.Name,
                            Address = companyData.Address,
                            PhoneNumber = companyData.Phone,
                            Website = companyData.Website,
                            TaxId = companyData.TaxId,
                            ContactEmail = companyData.Email,
                            CommercialRegister = "/uploads/commercial-register.pdf",
                            LogoUrl = "/uploads/logo.png",
                            Description = "Leading company in the industry",
                            IsVerified = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        context.Companies.Add(company);
                        hasNewCompanies = true;
                    }
                    continue;
                }

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
                        Description = "Leading company in the industry",
                        IsVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    context.Companies.Add(company);
                    hasNewCompanies = true;
                }
            }

            if (hasNewCompanies)
                await context.SaveChangesAsync();
        }

        public static async Task SeedMarketersAsync(UserManager<User> userManager, AffiliancesDBcontext context)
        {
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

            var random = new Random(42);
            var hasNewMarketers = false;

            foreach (var marketerData in marketers)
            {
                var existingUser = await userManager.FindByEmailAsync(marketerData.Email);

                if (existingUser != null)
                {
                    // User exists - ensure the Marketer record also exists
                    var marketerExists = await context.Marketers.AnyAsync(m => m.UserId == existingUser.Id);
                    if (!marketerExists)
                    {
                        var marketer = new Marketer
                        {
                            UserId = existingUser.Id,
                            Bio = marketerData.Bio,
                            Niche = marketerData.Niche,
                            SkillsExtracted = marketerData.Skills,
                            CvPath = "/uploads/cv.pdf",
                            NationalIdPath = "/uploads/national-id.pdf",
                            SocialLinks = "https://instagram.com/" + marketerData.FirstName.ToLower(),
                            TotalEarnings = random.Next(500, 10000),
                            PerformanceScore = (decimal)(random.NextDouble() * 2 + 3),
                            PersonalityScore = random.Next(60, 100),
                            PersonalityTestTaken = true,
                            IsVerified = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        context.Marketers.Add(marketer);
                        hasNewMarketers = true;
                    }
                    continue;
                }

                var user = new User
                {
                    UserName = marketerData.Email,
                    Email = marketerData.Email,
                    FirstName = marketerData.FirstName,
                    LastName = marketerData.LastName,
                    EmailConfirmed = true,
                    Status = UserStatus.Active,
                    Balance = random.Next(100, 5000),
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
                        TotalEarnings = random.Next(500, 10000),
                        PerformanceScore = (decimal)(random.NextDouble() * 2 + 3), // 3.0 to 5.0
                        PersonalityScore = random.Next(60, 100),
                        PersonalityTestTaken = true,
                        IsVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    context.Marketers.Add(marketer);
                    hasNewMarketers = true;
                }
            }

            if (hasNewMarketers)
                await context.SaveChangesAsync();
        }

        public static async Task SeedCampaignsAsync(AffiliancesDBcontext context)
        {
            var existingCount = await context.Campaigns.CountAsync();
            if (existingCount >= 10)
                return;

            // Remove old seed campaigns and their dependent data to re-seed
            if (existingCount > 0)
            {
                var oldCampaignIds = await context.Campaigns.Select(c => c.Id).ToListAsync();

                var oldApplications = await context.CampaignApplications
                    .Where(a => oldCampaignIds.Contains(a.CampaignId))
                    .ToListAsync();
                context.CampaignApplications.RemoveRange(oldApplications);

                var oldTrackingLinks = await context.TrackingLinks
                    .Where(t => oldCampaignIds.Contains(t.CampaignId))
                    .ToListAsync();
                context.TrackingLinks.RemoveRange(oldTrackingLinks);

                var oldReviews = await context.Reviews
                    .Where(r => r.CampaignId.HasValue && oldCampaignIds.Contains(r.CampaignId.Value))
                    .ToListAsync();
                context.Reviews.RemoveRange(oldReviews);

                context.Campaigns.RemoveRange(await context.Campaigns.ToListAsync());
                await context.SaveChangesAsync();
            }

            var companies = await context.Companies.ToListAsync();
            var categories = await context.Categories.Where(c => c.ParentId == null).ToListAsync();
            var subcategories = await context.Categories.Where(c => c.ParentId != null).ToListAsync();
            var admin = await context.Admins.FirstOrDefaultAsync();

            if (!companies.Any() || !categories.Any())
                return;

            var allCategories = categories.Concat(subcategories).ToList();
            var random = new Random(42);

            var campaignTemplates = new[]
            {
                // Technology
                new { Title = "Summer Tech Sale", Desc = "Promote our latest tech gadgets and earn up to 20% commission on every sale!", Category = "technology", Commission = CommissionType.Percentage, Value = 20m, Budget = 50000m },
                new { Title = "Software Launch Promo", Desc = "Help us launch our new SaaS product. Earn fixed rewards per signup.", Category = "software", Commission = CommissionType.Fixed, Value = 150m, Budget = 30000m },
                new { Title = "Gadget Review Program", Desc = "Review our latest smartphones and tablets. Earn commissions on referral purchases.", Category = "gadgets", Commission = CommissionType.Percentage, Value = 15m, Budget = 25000m },
                new { Title = "Cloud Services Campaign", Desc = "Promote our cloud hosting services to businesses and developers.", Category = "technology", Commission = CommissionType.Fixed, Value = 200m, Budget = 40000m },
                new { Title = "AI Tools Early Access", Desc = "Share our AI productivity tools with your audience. High conversion rates!", Category = "software", Commission = CommissionType.Percentage, Value = 25m, Budget = 35000m },
                // Fashion
                new { Title = "Winter Fashion Collection", Desc = "Showcase our exclusive winter collection. Earn per sale!", Category = "fashion", Commission = CommissionType.Percentage, Value = 12m, Budget = 20000m },
                new { Title = "Men's Streetwear Drop", Desc = "Promote the hottest streetwear for men. Limited edition items!", Category = "mens-fashion", Commission = CommissionType.Percentage, Value = 18m, Budget = 15000m },
                new { Title = "Women's Luxury Line", Desc = "Exclusive women's luxury fashion line. High ticket commissions.", Category = "womens-fashion", Commission = CommissionType.Percentage, Value = 22m, Budget = 45000m },
                new { Title = "Back to School Fashion", Desc = "School season is here! Promote our affordable kids and teen fashion.", Category = "fashion", Commission = CommissionType.Fixed, Value = 50m, Budget = 18000m },
                // Health & Beauty
                new { Title = "Skincare Routine Bundle", Desc = "Promote our best-selling skincare bundles. Great conversion rates!", Category = "skincare", Commission = CommissionType.Percentage, Value = 15m, Budget = 22000m },
                new { Title = "Makeup Masterclass Promo", Desc = "Share our online makeup masterclass. Earn per enrollment.", Category = "makeup", Commission = CommissionType.Fixed, Value = 75m, Budget = 12000m },
                new { Title = "Organic Health Products", Desc = "Promote our organic and natural health products line.", Category = "health-beauty", Commission = CommissionType.Percentage, Value = 17m, Budget = 28000m },
                new { Title = "Fitness Supplement Launch", Desc = "New protein and supplement line. Fitness influencers wanted!", Category = "health-beauty", Commission = CommissionType.Percentage, Value = 20m, Budget = 32000m },
                // Food & Beverage
                new { Title = "Gourmet Delivery Service", Desc = "Promote our premium food delivery. Earn per new customer signup!", Category = "food-beverage", Commission = CommissionType.Fixed, Value = 30m, Budget = 25000m },
                new { Title = "Healthy Meal Plans", Desc = "Share our weekly healthy meal subscription plans.", Category = "food-beverage", Commission = CommissionType.Percentage, Value = 10m, Budget = 15000m },
                new { Title = "Coffee Brand Ambassador", Desc = "Become an ambassador for our specialty coffee brand.", Category = "food-beverage", Commission = CommissionType.Percentage, Value = 14m, Budget = 20000m },
                // Travel
                new { Title = "Travel Booking Rewards", Desc = "Promote our travel booking platform. Earn on every booking!", Category = "travel", Commission = CommissionType.Percentage, Value = 8m, Budget = 60000m },
                new { Title = "Adventure Tours Campaign", Desc = "Share our adventure tour packages across the Middle East.", Category = "travel", Commission = CommissionType.Fixed, Value = 250m, Budget = 40000m },
                // Education
                new { Title = "Online Courses Promo", Desc = "Promote our online learning platform with 500+ courses.", Category = "education", Commission = CommissionType.Percentage, Value = 30m, Budget = 35000m },
                new { Title = "Language Learning App", Desc = "Share our language learning app. Earn per premium subscription.", Category = "education", Commission = CommissionType.Fixed, Value = 100m, Budget = 20000m },
                // Finance
                new { Title = "FinTech App Launch", Desc = "Promote our new mobile banking app. Earn per verified signup!", Category = "finance", Commission = CommissionType.Fixed, Value = 300m, Budget = 80000m },
                new { Title = "Investment Platform Referral", Desc = "Refer users to our investment platform. High-value commissions.", Category = "finance", Commission = CommissionType.Fixed, Value = 500m, Budget = 100000m },
                // Entertainment
                new { Title = "Streaming Service Promo", Desc = "Promote our streaming service. Earn on every new subscription!", Category = "entertainment", Commission = CommissionType.Percentage, Value = 20m, Budget = 45000m },
                new { Title = "Gaming Tournament Sponsor", Desc = "Spread the word about our gaming tournaments. Prizes + commissions!", Category = "entertainment", Commission = CommissionType.Fixed, Value = 120m, Budget = 30000m },
                new { Title = "E-book Platform Launch", Desc = "Promote our new e-book platform with thousands of titles.", Category = "entertainment", Commission = CommissionType.Percentage, Value = 25m, Budget = 18000m },
            };

            var campaigns = new List<Campaign>();
            var templateIndex = 0;

            foreach (var template in campaignTemplates)
            {
                // Round-robin assignment so every company gets campaigns
                var company = companies[templateIndex % companies.Count];
                templateIndex++;
                var category = allCategories.FirstOrDefault(c => c.Slug == template.Category)
                               ?? categories[random.Next(categories.Count)];

                // Vary the status: most active, some pending, a few completed/paused
                var statusRoll = random.Next(100);
                CampaignStatus status;
                DateTime startDate;
                DateTime endDate;
                int? approvedBy = null;

                if (statusRoll < 60) // 60% Active
                {
                    status = CampaignStatus.Active;
                    startDate = DateTime.UtcNow.AddDays(-random.Next(10, 60));
                    endDate = DateTime.UtcNow.AddDays(random.Next(30, 120));
                    approvedBy = admin?.UserId;
                }
                else if (statusRoll < 75) // 15% Pending
                {
                    status = CampaignStatus.Pending;
                    startDate = DateTime.UtcNow.AddDays(random.Next(1, 30));
                    endDate = startDate.AddDays(random.Next(30, 90));
                }
                else if (statusRoll < 85) // 10% Completed
                {
                    status = CampaignStatus.Completed;
                    startDate = DateTime.UtcNow.AddDays(-random.Next(90, 180));
                    endDate = DateTime.UtcNow.AddDays(-random.Next(1, 30));
                    approvedBy = admin?.UserId;
                }
                else if (statusRoll < 95) // 10% Paused
                {
                    status = CampaignStatus.Paused;
                    startDate = DateTime.UtcNow.AddDays(-random.Next(20, 60));
                    endDate = DateTime.UtcNow.AddDays(random.Next(10, 60));
                    approvedBy = admin?.UserId;
                }
                else // 5% Rejected
                {
                    status = CampaignStatus.Rejected;
                    startDate = DateTime.UtcNow.AddDays(random.Next(1, 15));
                    endDate = startDate.AddDays(random.Next(30, 90));
                    approvedBy = admin?.UserId;
                }

                campaigns.Add(new Campaign
                {
                    CompanyId = company.Id,
                    CategoryId = category.Id,
                    Title = template.Title,
                    Description = template.Desc,
                    CommissionType = template.Commission,
                    CommissionValue = template.Value,
                    Budget = template.Budget,
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = status,
                    TrackingBaseUrl = $"https://track.affiliance.com/{template.Category}",
                    PromotionalMaterials = "/uploads/promo-materials.zip",
                    ApprovedBy = approvedBy,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 90))
                });
            }

            context.Campaigns.AddRange(campaigns);
            await context.SaveChangesAsync();
        }

        public static async Task SeedCampaignApplicationsAsync(AffiliancesDBcontext context)
        {
            // Re-seed if campaigns were re-seeded (applications already cleared in SeedCampaignsAsync)
            if (await context.CampaignApplications.AnyAsync())
                return;

            var campaigns = await context.Campaigns.Where(c => c.Status == CampaignStatus.Active).ToListAsync();
            var marketers = await context.Marketers.ToListAsync();

            if (!campaigns.Any() || !marketers.Any())
                return;

            var applications = new List<CampaignApplication>();
            var random = new Random(123);

            foreach (var campaign in campaigns)
            {
                var selectedMarketers = marketers.OrderBy(x => random.Next()).Take(random.Next(2, Math.Min(6, marketers.Count + 1)));

                foreach (var marketer in selectedMarketers)
                {
                    var status = (ApplicationStatus)random.Next(0, 3);
                    applications.Add(new CampaignApplication
                    {
                        CampaignId = campaign.Id,
                        MarketerId = marketer.Id,
                        Status = status,
                        AiMatchScore = (decimal)(random.NextDouble() * 0.5 + 0.5),
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
            // Re-seed if campaigns were re-seeded (campaign-linked reviews already cleared in SeedCampaignsAsync)
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
            try
            {
                await SeedCategoriesAsync(context);
                await SeedCompaniesAsync(userManager, context);
                await SeedMarketersAsync(userManager, context);
                await SeedCampaignsAsync(context);
                await SeedCampaignApplicationsAsync(context);
                await SeedReviewsAsync(context);
            }
            catch (Exception)
            {
                // Ignore seed errors in deployment - data may already exist
            }
        }

        public static async Task ResetAndReseedAsync(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager, AffiliancesDBcontext context)
        {
            // 1. Delete all seeded data in correct order (respect FK constraints)
            context.PerformanceLogs.RemoveRange(await context.PerformanceLogs.ToListAsync());
            context.TrackingLinks.RemoveRange(await context.TrackingLinks.ToListAsync());
            context.CampaignApplications.RemoveRange(await context.CampaignApplications.ToListAsync());
            context.Reviews.RemoveRange(await context.Reviews.ToListAsync());
            context.Complaints.RemoveRange(await context.Complaints.ToListAsync());
            context.AiSuggestions.RemoveRange(await context.AiSuggestions.ToListAsync());
            context.Payments.RemoveRange(await context.Payments.ToListAsync());
            context.WithdrawalRequests.RemoveRange(await context.WithdrawalRequests.ToListAsync());
            context.PaymentMethods.RemoveRange(await context.PaymentMethods.ToListAsync());
            context.Notifications.RemoveRange(await context.Notifications.ToListAsync());
            context.NotificationPreferences.RemoveRange(await context.NotificationPreferences.ToListAsync());
            context.AuditLogs.RemoveRange(await context.AuditLogs.ToListAsync());
            context.Campaigns.RemoveRange(await context.Campaigns.ToListAsync());
            context.Categories.RemoveRange(await context.Categories.ToListAsync());
            await context.SaveChangesAsync();

            // 2. Delete profile entities
            context.Marketers.RemoveRange(await context.Marketers.ToListAsync());
            context.Companies.RemoveRange(await context.Companies.ToListAsync());
            context.Admins.RemoveRange(await context.Admins.ToListAsync());
            await context.SaveChangesAsync();

            // 3. Delete all users
            var allUsers = await context.Users.ToListAsync();
            foreach (var user in allUsers)
            {
                await userManager.DeleteAsync(user);
            }

            // 4. Re-seed everything
            await SeedRolesAsync(roleManager);
            await SeedAdminAsync(userManager, context);
            await SeedAllDataAsync(userManager, context);
        }
    }
}
