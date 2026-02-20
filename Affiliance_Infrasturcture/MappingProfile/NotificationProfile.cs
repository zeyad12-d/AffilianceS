using Affiliance_core.Dto.NotificationDto;
using Affiliance_core.Entites;
using AutoMapper;

namespace Affiliance_Infrasturcture.MappingProfile
{
    public class NotificationProfile : Profile
    {
        public NotificationProfile()
        {
            CreateMap<Notification, NotificationListDto>();

            CreateMap<NotificationPreference, NotificationPreferenceDto>()
                .ForMember(dest => dest.NotificationTypeDisplay, opt => opt.MapFrom(src => src.NotificationType.ToString()));
        }
    }
}
