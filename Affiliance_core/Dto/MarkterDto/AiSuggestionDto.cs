namespace Affiliance_core.Dto.MarkterDto
{
    public class AiSuggestionDto
    {
        public int CampaignId { get; set; }
        public string CampaignTitle { get; set; }
        public decimal MatchScore { get; set; }
        public string MatchReason { get; set; }
    }
}
