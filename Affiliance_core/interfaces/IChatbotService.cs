using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.ChatbotDto;

namespace Affiliance_core.interfaces
{
    public interface IChatbotService
    {
        Task<ApiResponse<ChatbotResponseDto>> SendMessageAsync(ChatbotRequestDto request);
    }
}
