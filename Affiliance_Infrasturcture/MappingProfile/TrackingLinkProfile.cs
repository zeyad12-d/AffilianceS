using Affiliance_core.Dto.TrackingLinkDto;
using Affiliance_core.Entites;

namespace Affiliance_Infrasturcture.MappingProfile
{
    public class TrackingLinkProfile : AutoMapper.Profile
    {
        public TrackingLinkProfile()
        {
            CreateMap<TrackingLink, TrackingLinkDto>()
                .ForMember(dest => dest.CampaignTitle, opt => opt.MapFrom(src => src.Campaign.Title));
        }
    }
}
