using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.CampanyDto;
using Affiliance_core.Dto.MarkterDto;
using Affiliance_core.Entites;
using Affiliance_core.interfaces;
using Affiliance_Infrasturcture.Services;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Affiliance_Applaction.services
{
    public class CampanyService : ICampanyServices
    {
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        public CampanyService(UserManager<User> userManager, IUnitOfWork unitOfWork ,IMapper mapper,IFileService fileService)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileService = fileService;

        }

      
        #region Register Company
        public async Task<ApiResponse<string>> RegisterCompanyAsync(CompanyRegisterDto dto)
        {
            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Status = UserStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return ApiResponse<string>.CreateFail(string.Join(", ", result.Errors.Select(e => e.Description)));

            try
            {
                await _userManager.AddToRoleAsync(user, "Company");

                var company = _mapper.Map<Company>(dto);
                company.UserId = user.Id;


                company.CommercialRegister = await _fileService.SaveFileAsync(dto.CommercialRegisterFile, "CommercialRegisters");

                if (dto.LogoFile != null)
                {
                    company.LogoUrl = await _fileService.SaveFileAsync(dto.LogoFile, "Logos");
                }

                await _unitOfWork.Repository<Company>().AddAsync(company);
                await _unitOfWork.CompleteAsync();

                return ApiResponse<string>.CreateSuccess("تم تسجيل الشركة بنجاح، بانتظار مراجعة الإدارة.");
            }
            catch (Exception ex)
            {
                await _userManager.DeleteAsync(user);
                return ApiResponse<string>.CreateFail("حدث خطأ أثناء تسجيل الشركة: " + ex.Message);
            }
        }

        #endregion


    
    }
}
