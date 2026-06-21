using LanAnhComputer.API.Dtos;
using LanAnhComputer.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LanAnhComputer.API.Controllers
{
    [ApiController]
    [Route("api/chatbot")]
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

            var response = await _chatbotService.HandleChatAsync(request);

            return Ok(new { response = response });
        }
    }
}
