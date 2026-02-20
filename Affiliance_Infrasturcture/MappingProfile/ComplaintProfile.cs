using Affiliance_core.Dto.ComplaintDto;
using Affiliance_core.Entites;
using AutoMapper;

namespace Affiliance_Infrasturcture.MappingProfile
{
    public class ComplaintProfile : Profile
    {
        public ComplaintProfile()
        {
            CreateMap<Complaint, ComplaintDto>()
                .ForMember(dest => dest.ComplainantName, opt => opt.MapFrom(src => src.Complainant.FirstName + " " + src.Complainant.LastName))
                .ForMember(dest => dest.DefendantName, opt => opt.MapFrom(src => src.Defendant.FirstName + " " + src.Defendant.LastName))
                .ForMember(dest => dest.CampaignTitle, opt => opt.MapFrom(src => src.Campaign != null ? src.Campaign.Title : null))
                .ForMember(dest => dest.StatusDisplay, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ResolvedByName, opt => opt.MapFrom(src => src.ResolvedByNavigation != null ? src.ResolvedByNavigation.FirstName + " " + src.ResolvedByNavigation.LastName : null));

            CreateMap<Complaint, ComplaintDetailsDto>()
                .ForMember(dest => dest.ComplainantName, opt => opt.MapFrom(src => src.Complainant.FirstName + " " + src.Complainant.LastName))
                .ForMember(dest => dest.ComplainantEmail, opt => opt.MapFrom(src => src.Complainant.Email))
                .ForMember(dest => dest.DefendantName, opt => opt.MapFrom(src => src.Defendant.FirstName + " " + src.Defendant.LastName))
                .ForMember(dest => dest.DefendantEmail, opt => opt.MapFrom(src => src.Defendant.Email))
                .ForMember(dest => dest.CampaignTitle, opt => opt.MapFrom(src => src.Campaign != null ? src.Campaign.Title : null))
                .ForMember(dest => dest.StatusDisplay, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ResolvedByName, opt => opt.MapFrom(src => src.ResolvedByNavigation != null ? src.ResolvedByNavigation.FirstName + " " + src.ResolvedByNavigation.LastName : null));

            CreateMap<CreateComplaintDto, Complaint>();
        }
    }
}
