using Affiliance_core.Dto.CategoryDto;
using Affiliance_core.Entites;

namespace Affiliance_Infrasturcture.MappingProfile
{
    public class CategoryProfile : AutoMapper.Profile
    {
        public CategoryProfile()
        {
            // Category -> CategoryDto
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.NameEn : null))
                .ForMember(dest => dest.ChildrenCount, opt => opt.MapFrom(src => src.Children != null ? src.Children.Count : 0))
                .ForMember(dest => dest.CampaignsCount, opt => opt.MapFrom(src => src.Campaigns != null ? src.Campaigns.Count : 0));

            // CreateCategoryDto -> Category
            CreateMap<CreateCategoryDto, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore())
                .ForMember(dest => dest.Campaigns, opt => opt.Ignore());

            // UpdateCategoryDto -> Category (for updates)
            CreateMap<UpdateCategoryDto, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.NameEn, opt => opt.Condition(src => !string.IsNullOrEmpty(src.NameEn)))
                .ForMember(dest => dest.NameAr, opt => opt.Condition(src => !string.IsNullOrEmpty(src.NameAr)))
                .ForMember(dest => dest.Slug, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Slug)))
                .ForMember(dest => dest.Icon, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Icon)))
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore())
                .ForMember(dest => dest.Campaigns, opt => opt.Ignore());
        }
    }
}
