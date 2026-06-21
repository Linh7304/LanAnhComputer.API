using LanAnhComputer.API.Dtos;
using LanAnhComputer.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace LanAnhComputer.Web.Controllers
{
    [ApiController]
    [Route("chatbot")]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message is required.");
            }

            // Extract User ID from Session if logged in
            var token = HttpContext.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(token))
            {
                // In a real scenario, you could decode the JWT to get UserId or store it in Session
                var userIdStr = HttpContext.Session.GetString("UserId");
                if (int.TryParse(userIdStr, out int userId))
                {
                    request.UserId = userId;
                }
            }

            var response = await _chatbotService.HandleChatAsync(request);

            return Ok(new { response = response });
        }
    }
}
