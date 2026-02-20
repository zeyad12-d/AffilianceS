using Affiliance_core.Dto.CampaignDto;
using Affiliance_core.Dto.CategoryDto;
using Affiliance_core.Entites;
using AutoMapper;

namespace Affiliance_Infrasturcture.MappingProfile
{
    public class CampaignProfile : Profile
    {
        public CampaignProfile()
        {
            // Campaign Entity -> CampaignDto
            CreateMap<Campaign, CampaignDto>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.CampanyName))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.NameEn))
                .ForMember(dest => dest.ApplicationsCount, opt => opt.MapFrom(src => src.CampaignApplications.Count))
                .ForMember(dest => dest.AcceptedApplicationsCount, opt => opt.MapFrom(src => src.CampaignApplications.Count(a => a.Status == ApplicationStatus.Accepted)))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedByNavigation != null 
                    ? src.ApprovedByNavigation.FirstName + " " + src.ApprovedByNavigation.LastName 
                    : null));

            // Campaign Entity -> CampaignDetailsDto
            CreateMap<Campaign, CampaignDetailsDto>()
                .IncludeBase<Campaign, CampaignDto>()
                .ForMember(dest => dest.Company, opt => opt.MapFrom(src => src.Company))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.Statistics, opt => opt.Ignore()); // Will be populated separately

            // Company -> CompanyBasicDto
            CreateMap<Company, CompanyBasicDto>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.CampanyName));

            // CreateCampaignDto -> Campaign
            CreateMap<CreateCampaignDto, Campaign>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => CampaignStatus.Pending))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedByNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.CampaignApplications, opt => opt.Ignore())
                .ForMember(dest => dest.TrackingLinks, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                .ForMember(dest => dest.Complaints, opt => opt.Ignore())
                .ForMember(dest => dest.AiSuggestions, opt => opt.Ignore());

            // UpdateCampaignDto -> Campaign (only update non-null values)
            CreateMap<UpdateCampaignDto, Campaign>()
                .ForMember(dest => dest.Title, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Title)))
                .ForMember(dest => dest.Description, opt => opt.Condition(src => src.Description != null))
                .ForMember(dest => dest.CategoryId, opt => opt.Condition(src => src.CategoryId.HasValue))
                .ForMember(dest => dest.CommissionType, opt => opt.Condition(src => src.CommissionType.HasValue))
                .ForMember(dest => dest.CommissionValue, opt => opt.Condition(src => src.CommissionValue.HasValue))
                .ForMember(dest => dest.Budget, opt => opt.Condition(src => src.Budget.HasValue))
                .ForMember(dest => dest.StartDate, opt => opt.Condition(src => src.StartDate.HasValue))
                .ForMember(dest => dest.EndDate, opt => opt.Condition(src => src.EndDate.HasValue))
                .ForMember(dest => dest.PromotionalMaterials, opt => opt.Condition(src => src.PromotionalMaterials != null))
                .ForMember(dest => dest.TrackingBaseUrl, opt => opt.Condition(src => src.TrackingBaseUrl != null))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
