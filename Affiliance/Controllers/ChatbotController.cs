using Affiliance_core.ApiHelper;
using Affiliance_core.Dto.ChatbotDto;
using Affiliance_core.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Affiliance_Api.Controllers
{
    /// <summary>
    /// Controller for chatbot interactions.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly IServicesManager _servicesManager;

        public ChatbotController(IServicesManager servicesManager)
        {
            _servicesManager = servicesManager;
        }

        /// <summary>
        /// Sends a message to the chatbot and returns the response.
        /// Accepts text, image, and/or audio inputs.
        /// </summary>
        /// <param name="request">The chatbot request containing text, image, and/or audio.</param>
        /// <returns>Returns the chatbot response.</returns>
        [HttpPost("send")]
        [Authorize]
        public async Task<IActionResult> SendMessage([FromForm] ChatbotRequestDto request)
        {
            if (request is null)
                return BadRequest("Request body is required.");

            var result = await _servicesManager.ChatbotService.SendMessageAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
