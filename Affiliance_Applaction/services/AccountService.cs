using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.AccountDto;
using Affiliance_core.Entites;
using Affiliance_core.interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Affiliance_Applaction.services
{
    public class AccountService : IAccountService
    {
        private const string InvalidInputMessage = "Invalid input.";
        private const string UserNotFoundMessage = "User Not Found By This Email";
        private const string InvalidPasswordMessage = "Invalid Password";
        private const string LoginSuccessMessage = "Login Successful";
        private const string EmailExistsMessage = "User with this email already exists.";
        private const string RegistrationFailedMessage = "Registration failed.";
        private const string LoginFailedMessage = "Login failed.";
        private readonly UserManager<User> _userManager;
        private readonly IServicesManager _servicesManager; 
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly IConfiguration _configuration;

        public AccountService(
            UserManager<User> userManager,
            IServicesManager servicesManager,
            IUnitOfWork unitOfWork,
            IFileService fileService, IConfiguration configuration)
        {
            _userManager = userManager;
            _servicesManager = servicesManager;
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _configuration = configuration;
        }
        #region LoignAsync
        public async Task<ApiResponse<AuthModel>> LoginMarketerAsync(LoginDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return ApiResponse<AuthModel>.CreateFail(InvalidInputMessage);
            }

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) { return ApiResponse<AuthModel>.CreateFail(UserNotFoundMessage); }
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isPasswordValid) { return ApiResponse<AuthModel>.CreateFail(InvalidPasswordMessage); }
           
            var roles = await _userManager.GetRolesAsync(user);
            var authClaims = new List<Claim> 
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                 new Claim(ClaimTypes.Email, user.Email),
               new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach(var role in roles) { authClaims.Add(new Claim(ClaimTypes.Role, role)); }

            // Add marketerId claim for Marketer role
            if (roles.Contains("Marketer"))
            {
                var marketers = await _unitOfWork.Repository<Marketer>().FindAsync(m => m.UserId == user.Id);
                var marketer = marketers.FirstOrDefault();
                if (marketer != null)
                {
                    authClaims.Add(new Claim("marketerId", marketer.Id.ToString()));
                }
            }

            // Add companyId claim for Company role
            if (roles.Contains("Company"))
            {
                var companies = await _unitOfWork.Repository<Company>().FindAsync(c => c.UserId == user.Id);
                var company = companies.FirstOrDefault();
                if (company != null)
                {
                    authClaims.Add(new Claim("companyId", company.Id.ToString()));
                }
            }

          var Token = CreateJwtToken(authClaims);
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(Token);
            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiry = GetRefreshTokenExpiryTime();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = refreshTokenExpiry;
            user.LastLoginAt = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return ApiResponse<AuthModel>.CreateFail(LoginFailedMessage);
            }

            var authModel = new AuthModel
            {
                IsAuthenticated = true,
                Token = jwtToken,
                ExpiresOn = Token.ValidTo,
                Message = LoginSuccessMessage,
                RefreshToken = refreshToken,
                RefreshTokenExpiresOn = refreshTokenExpiry
            };
            return ApiResponse<AuthModel>.CreateSuccess(authModel, LoginSuccessMessage);
        }

       

        #endregion

        #region RegisterMarketerAsync
        public async Task<ApiResponse<string>> RegisterMarketerAsync(MarketerRegisterDto dto)
        {
            if (dto == null
                || string.IsNullOrWhiteSpace(dto.Email)
                || string.IsNullOrWhiteSpace(dto.Password)
                || dto.NationalIdImage == null)
            {
                return ApiResponse<string>.CreateFail(InvalidInputMessage);
            }

            
            var aiResult = await _servicesManager.AiService.AnalyzeImageAsync(dto.NationalIdImage);
            if (aiResult != "Success")
            {
                return ApiResponse<string>.CreateFail($"ID Validation Failed: {aiResult}");
            }

           
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return ApiResponse<string>.CreateFail(EmailExistsMessage);
            }

          
            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FullName,
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

          

            try
            {
               
                await _userManager.AddToRoleAsync(user, "Marketer");

             
                var idPath = await _fileService.SaveFileAsync(dto.NationalIdImage, "marketer_ids");

                
                var marketer = new Marketer
                {
                    UserId = user.Id,
                    NationalIdPath = idPath,
                    IsVerified = true, 
                    
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
              
                await _userManager.DeleteAsync(user);
                return ApiResponse<string>.CreateFail(RegistrationFailedMessage);
            }

            return ApiResponse<string>.CreateSuccess(user.Id.ToString(), "Marketer registered successfully.");
        }
        #endregion

        #region Token Generation Methods
        private JwtSecurityToken CreateJwtToken(List<Claim> authClaims)
        {
         
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));

            int.TryParse(_configuration["JwtSettings:DurationInDays"], out int durationInDays);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                expires: DateTime.UtcNow.AddDays(durationInDays == 0 ? 30 : durationInDays), // Default 30 days
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomNumber);
        }

        private DateTime GetRefreshTokenExpiryTime()
        {
            int.TryParse(_configuration["JwtSettings:RefreshTokenDurationInDays"], out int durationInDays);
            return DateTime.UtcNow.AddDays(durationInDays == 0 ? 7 : durationInDays);
        }
        #endregion

        public async Task<ApiResponse<bool>> LogoutAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return ApiResponse<bool>.CreateFail("User ID is required.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<bool>.CreateFail("User not found.");
            }

            // Invalidate refresh token to prevent new access tokens from being issued
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = DateTime.MinValue;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return ApiResponse<bool>.CreateFail("Logout failed.");
            }

            return ApiResponse<bool>.CreateSuccess(true, "Logged out successfully.");
        }


        public async Task<ApiResponse<bool>> ChangePasswordAsync(ChangePasswordDto dto)
        {
            if (dto == null ||
                  string.IsNullOrWhiteSpace(dto.UserId) ||
        string.IsNullOrWhiteSpace(dto.CurrentPassword) ||
        string.IsNullOrWhiteSpace(dto.NewPassword)
                )
            {
                return ApiResponse<bool>.CreateFail("InVaild input");
            }
            var User = await _userManager.FindByIdAsync(dto.UserId);
            if(User == null)
            {
               return ApiResponse<bool>.CreateFail("User Not Found");
            }

            var result = await _userManager.ChangePasswordAsync(
                User,
                dto.CurrentPassword,
                dto.NewPassword
            );

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse<bool>.CreateFail($"Password change failed: {errors}");
            }

            return ApiResponse<bool>.CreateSuccess(true, "Password changed successfully");

        }

    }
}
