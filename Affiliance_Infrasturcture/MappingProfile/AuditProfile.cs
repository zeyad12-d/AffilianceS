using Affiliance_core.Dto.AuditDto;
using Affiliance_core.Entites;
using AutoMapper;

namespace Affiliance_Infrasturcture.MappingProfile
{
    public class AuditProfile : Profile
    {
        public AuditProfile()
        {
            CreateMap<AuditLog, AuditLogDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FirstName + " " + src.User.LastName : null));
        }
    }
}
