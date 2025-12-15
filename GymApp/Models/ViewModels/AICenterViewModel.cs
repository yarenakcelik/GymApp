using GymApp.Services;

namespace GymApp.Models.ViewModels
{
    public class AICenterViewModel
    {
        public AiService.AiRequestDto Request { get; set; } = new AiService.AiRequestDto();

        // Çıktı ve hata
        public string? Result { get; set; }
        public string? Error { get; set; }
    }
}
