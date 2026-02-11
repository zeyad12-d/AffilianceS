using Affiliance_core.Dto.MarkterDto;
using Affiliance_core.Entites;

namespace Affiliance_Infrasturcture.MappingProfile
{
    public class MarketerProfile : AutoMapper.Profile
    {
        public MarketerProfile()
        {
            CreateMap<Marketer, MarketerProfileDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom(src => src.User.ProfilePicture))
                .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.TotalEarnings));

            CreateMap<Marketer, MarketerPublicDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom(src => src.User.ProfilePicture))
                .ForMember(dest => dest.AverageRating, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewsCount, opt => opt.Ignore());

            CreateMap<UpdateMarketerProfileDto, Marketer>()
                .ForMember(dest => dest.Bio, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Bio)))
                .ForMember(dest => dest.Niche, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Niche)))
                .ForMember(dest => dest.SocialLinks, opt => opt.Condition(src => !string.IsNullOrEmpty(src.SocialLinks)))
                .ForMember(dest => dest.SkillsExtracted, opt => opt.Condition(src => !string.IsNullOrEmpty(src.SkillsExtracted)));

            CreateMap<CampaignApplication, CampaignApplicationDto>()
                .ForMember(dest => dest.CampaignTitle, opt => opt.MapFrom(src => src.Campaign.Title))
                .ForMember(dest => dest.MarketerName, opt => opt.MapFrom(src => src.Marketer.User.FirstName + " " + src.Marketer.User.LastName));

            CreateMap<AiSuggestion, AiSuggestionDto>()
                .ForMember(dest => dest.CampaignTitle, opt => opt.MapFrom(src => src.Campaign.Title))
                .ForMember(dest => dest.MatchReason, opt => opt.MapFrom(src => src.Reason));
        }
    }
}
