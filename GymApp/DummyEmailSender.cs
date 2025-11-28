using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace GymApp
{
    // Gerçek mail göndermeyen, sadece Identity'yi memnun eden sahte servis
    public class DummyEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // İstersen burada log yazdırabilirsin, şimdilik hiçbir şey yapmıyoruz.
            return Task.CompletedTask;
        }
    }
}
