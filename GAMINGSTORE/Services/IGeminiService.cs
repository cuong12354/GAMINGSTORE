namespace GAMINGSTORE.Services
{
    public interface IGeminiService
    {
        Task<string> GetConsultationAsync(string userMessage, string userId);
    }
}
