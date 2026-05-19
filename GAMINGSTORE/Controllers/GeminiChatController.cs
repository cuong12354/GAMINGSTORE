using GAMINGSTORE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GAMINGSTORE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GeminiChatController : ControllerBase
    {
        private readonly IGeminiService _geminiService;
        private readonly ILogger<GeminiChatController> _logger;

        public GeminiChatController(IGeminiService geminiService, ILogger<GeminiChatController> logger)
        {
            _geminiService = geminiService;
            _logger = logger;
        }

        [HttpPost("consult")]
        public async Task<IActionResult> GetConsultation([FromBody] ConsultationRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Vui lòng đăng nhập" });

                if (string.IsNullOrEmpty(request?.Message))
                    return BadRequest(new { message = "Vui lòng nhập câu hỏi" });

                var response = await _geminiService.GetConsultationAsync(request.Message, userId);

                return Ok(new { message = response });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi trong GetConsultation: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi máy chủ" });
            }
        }

        public class ConsultationRequest
        {
            public string? Message { get; set; }
        }
    }
}
