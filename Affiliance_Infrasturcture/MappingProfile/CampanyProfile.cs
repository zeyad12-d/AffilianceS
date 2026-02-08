using Affiliance_core.Dto.AccountDto;
using Affiliance_core.Entites;

namespace Affiliance_Infrasturcture.MappingProfile
{
    public class CampanyProfile:AutoMapper.Profile
    {

        public CampanyProfile()
        {
            CreateMap<CompanyRegisterDto, Company>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.CommercialRegister, opt => opt.Ignore())
                .ForMember(dest => dest.LogoUrl, opt => opt.Ignore())
                .ForMember(dest => dest.IsVerified, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Description, opt => opt.Ignore())
                .ForMember(dest => dest.CampanyName, opt => opt.MapFrom(src => src.CompanyName))
                .ForMember(dest => dest.ContactEmail, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Website, opt => opt.MapFrom(src => src.Website))
                .ForMember(dest => dest.TaxId, opt => opt.MapFrom(src => src.TaxId));

        }
    }
}
