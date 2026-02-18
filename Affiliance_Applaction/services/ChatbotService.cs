using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.ChatbotDto;
using Affiliance_core.interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Affiliance_Applaction.services
{
    public class ChatbotService : IChatbotService
    {
        private readonly HttpClient _httpClient;
        private readonly string _chatbotBaseUrl;

        public ChatbotService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _chatbotBaseUrl = configuration["ChatbotSettings:BaseUrl"]
                ?? "https://api-chatbot-production-12c0.up.railway.app";
        }

        public async Task<ApiResponse<ChatbotResponseDto>> SendMessageAsync(ChatbotRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Text) && request.Image is null && request.Audio is null)
                    return ApiResponse<ChatbotResponseDto>.CreateFail("??? ????? ?? ?? ???? ?? ??? ??? ?????.");

                using var content = new MultipartFormDataContent();

                content.Add(new StringContent(request.Text ?? string.Empty), "text");

                if (request.Image is not null && request.Image.Length > 0)
                {
                    var imageStream = new StreamContent(request.Image.OpenReadStream());
                    imageStream.Headers.ContentType = new MediaTypeHeaderValue(request.Image.ContentType);
                    content.Add(imageStream, "image", request.Image.FileName);
                }
                else
                {
                    content.Add(new StringContent(string.Empty), "image");
                }

                if (request.Audio is not null && request.Audio.Length > 0)
                {
                    var audioStream = new StreamContent(request.Audio.OpenReadStream());
                    audioStream.Headers.ContentType = new MediaTypeHeaderValue(request.Audio.ContentType);
                    content.Add(audioStream, "audio", request.Audio.FileName);
                }
                else
                {
                    content.Add(new StringContent(string.Empty), "audio");
                }

                var response = await _httpClient.PostAsync($"{_chatbotBaseUrl}/chat", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return ApiResponse<ChatbotResponseDto>.CreateFail(
                        $"??? ??????? ?????? ???. Status: {(int)response.StatusCode}. Details: {responseBody}");
                }

                var result = JsonSerializer.Deserialize<ChatbotResponseDto>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result is null || string.IsNullOrWhiteSpace(result.Response))
                    return ApiResponse<ChatbotResponseDto>.CreateFail("?? ??? ?????? ?? ?? ????? ???.");

                return ApiResponse<ChatbotResponseDto>.CreateSuccess(result, "?? ?????? ???? ?????.");
            }
            catch (TaskCanceledException)
            {
                return ApiResponse<ChatbotResponseDto>.CreateFail("????? ???? ??????? ?????? ???. ???? ??? ????.");
            }
            catch (HttpRequestException ex)
            {
                return ApiResponse<ChatbotResponseDto>.CreateFail($"??? ?? ??????? ?????? ???: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ApiResponse<ChatbotResponseDto>.CreateFail($"??? ??? ??? ?????: {ex.Message}");
            }
        }
    }
}
