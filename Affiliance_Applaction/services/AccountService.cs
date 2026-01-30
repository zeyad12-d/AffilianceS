using System;
using System.Threading.Tasks;
using Affiliance_core.ApiHelper;
using Affiliance_core.Dto;
using Affiliance_core.Entites;
using Affiliance_core.interfaces;
using Microsoft.AspNetCore.Identity;

namespace Affiliance_Applaction.services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<User> _userManager;
        private readonly IServicesManager _servicesManager; // Contains AiService
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;

        public AccountService(
            UserManager<User> userManager,
            IServicesManager servicesManager,
            IUnitOfWork unitOfWork,
            IFileService fileService)
        {
            _userManager = userManager;
            _servicesManager = servicesManager;
            _unitOfWork = unitOfWork;
            _fileService = fileService;
        }

        public async Task<ApiResponse<string>> RegisterMarketerAsync(MarketerRegisterDto dto)
        {
            // 1. Validate ID using AI Service
            var aiResult = await _servicesManager.AiService.AnalyzeImageAsync(dto.NationalIdImage);
            if (aiResult != "Success")
            {
                return ApiResponse<string>.CreateFail($"ID Validation Failed: {aiResult}");
            }

            // 2. Check if user exists
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return ApiResponse<string>.CreateFail("User with this email already exists.");
            }

            // 3. Create Application User (Identity)
            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FullName, // Storing full name in FirstName for simplicity based on DTO, or split it
                PhoneNumber = dto.PhoneNumber,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return ApiResponse<string>.CreateFail($"User creation failed: {errors}");
            }

            // 4. Start Transaction to ensure data consistency
            // Note: Since Identity is handled separately usually, but we want to ensure Marketer creation succeeds.
            // If Marketer creation fails, we might want to delete the user or handle it.
            // For now, let's proceed with Marketer creation.

            try
            {
                // 5. Add to Role
                await _userManager.AddToRoleAsync(user, "Marketer");

                // 6. Save National ID Image
                var idPath = await _fileService.SaveFileAsync(dto.NationalIdImage, "marketer_ids");

                // 7. Create Marketer Entity
                var marketer = new Marketer
                {
                    UserId = user.Id,
                    NationalIdPath = idPath,
                    IsVerified = true, // Verified because AI returned "Success" ? Or wait for manual? Logic says "Validate... if not Success return error", so valid means potentially verified or at least AI-passed. Let's set it to true or false. Prompt doesn't specify logic for IsVerified value, but entity has IsVerified. I will set it to false (default) or true. Let's assume verified by AI = true for this flow or leave it default. I'll set it to true since AI check passed.
                    
                    // Initialize other required fields with defaults
                    Bio = string.Empty,
                    Niche = string.Empty,
                    CvPath = string.Empty,
                    SocialLinks = string.Empty,
                    SkillsExtracted = string.Empty
                };

                await _unitOfWork.Repository<Marketer>().AddAsync(marketer);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                // Rollback user creation if marketer creation fails
                await _userManager.DeleteAsync(user);
                return ApiResponse<string>.CreateFail($"Registration failed: {ex.Message}");
            }

            return ApiResponse<string>.CreateSuccess(user.Id.ToString(), "Marketer registered successfully.");
        }
    }
}
