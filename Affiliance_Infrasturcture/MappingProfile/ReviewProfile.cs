using Affiliance_core.Dto.ReviewDto;
using Affiliance_core.Entites;

namespace Affiliance_Infrasturcture.MappingProfile
{
    public class ReviewProfile : AutoMapper.Profile
    {
        public ReviewProfile()
        {
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.ReviewerName, opt => opt.MapFrom(src => src.Reviewer.FirstName + " " + src.Reviewer.LastName))
                .ForMember(dest => dest.CampaignTitle, opt => opt.MapFrom(src => src.Campaign != null ? src.Campaign.Title : null));
        }
    }
}
