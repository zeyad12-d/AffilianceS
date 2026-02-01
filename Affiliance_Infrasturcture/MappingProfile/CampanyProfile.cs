namespace Affiliance_Infrasturcture.MappingProfile
{
    public class CampanyProfile:AutoMapper.Profile
    {

        public CampanyProfile()
        {
            CreateMap<Affiliance_core.Dto.CampanyDto.CompanyRegisterDto, Affiliance_core.Entites.Company>()
                .ForMember(dest => dest.CommercialRegister, opt => opt.Ignore())
                .ForMember(dest => dest.LogoUrl, opt => opt.Ignore());
        }
    }
}
