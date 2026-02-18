using Affiliance_core.Dto.CategoryDto;

namespace Affiliance_core.Dto.CampaignDto
{
    public class CampaignDetailsDto : CampaignDto
    {
        public CompanyBasicDto Company { get; set; }
        public CategoryDto.CategoryDto Category { get; set; }
        public CampaignStatisticsDto? Statistics { get; set; }
    }
}
