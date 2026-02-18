using Microsoft.AspNetCore.Http;

namespace Affiliance_core.Dto.ChatbotDto
{
    public class ChatbotRequestDto
    {
        public string? Text { get; set; }
        public IFormFile? Image { get; set; }
        public IFormFile? Audio { get; set; }
    }
}
