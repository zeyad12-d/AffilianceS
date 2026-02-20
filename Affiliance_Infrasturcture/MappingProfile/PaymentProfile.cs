using Affiliance_core.Dto.PaymentDto;
using Affiliance_core.Entites;
using AutoMapper;

namespace Affiliance_Infrasturcture.MappingProfile
{
    public class PaymentProfile : Profile
    {
        public PaymentProfile()
        {
            CreateMap<Payment, PaymentDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName))
                .ForMember(dest => dest.CampaignTitle, opt => opt.MapFrom(src => src.Campaign != null ? src.Campaign.Title : null))
                .ForMember(dest => dest.TypeDisplay, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.StatusDisplay, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<CreatePaymentDto, Payment>();

            CreateMap<WithdrawalRequest, WithdrawalRequestDto>()
                .ForMember(dest => dest.MarketerName, opt => opt.MapFrom(src => src.Marketer.User.FirstName + " " + src.Marketer.User.LastName))
                .ForMember(dest => dest.PaymentMethodType, opt => opt.MapFrom(src => src.PaymentMethod.Type.ToString()))
                .ForMember(dest => dest.PaymentMethodDetails, opt => opt.MapFrom(src => src.PaymentMethod.AccountDetails))
                .ForMember(dest => dest.StatusDisplay, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ProcessedByName, opt => opt.MapFrom(src => src.ProcessedByUser != null ? src.ProcessedByUser.FirstName + " " + src.ProcessedByUser.LastName : null));

            CreateMap<PaymentMethod, PaymentMethodDto>()
                .ForMember(dest => dest.TypeDisplay, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.MaskedAccountInfo, opt => opt.MapFrom(src => "****" + (src.AccountDetails.Length > 4 ? src.AccountDetails.Substring(src.AccountDetails.Length - 4) : "")));

            CreateMap<CreatePaymentMethodDto, PaymentMethod>();
        }
    }
}
