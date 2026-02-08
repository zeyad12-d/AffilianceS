using Affiliance_core.Dto.AccountDto;
using Affiliance_core.Entites;

namespace Affiliance_Infrasturcture.MappingProfile
{
    public class CampanyProfile:AutoMapper.Profile
    {

        public CampanyProfile()
        {
            CreateMap<CompanyRegisterDto, Company>()
                .ForMember(dest => dest.CommercialRegister, opt => opt.Ignore())
                .ForMember(dest => dest.LogoUrl, opt => opt.Ignore()); 
        }
    }
}
