using GAMINGSTORE.Models;

namespace GAMINGSTORE.Services
{
    public interface IGeminiService
    {
        Task<ChatResponseDto> GetConsultationAsync(string userMessage, string? userId = null);
    }

    public class ChatResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public List<string> Suggestions { get; set; } = new();
        public List<Product> Products { get; set; } = new();
    }
}
