using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class MemberProfile
    {
        public int Id { get; set; }

        // Hangi Identity kullanıcısına ait?
        [Required]
        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser ApplicationUser { get; set; } = null!;

        [StringLength(100)]
        public string? FullName { get; set; }

        public double? HeightCm { get; set; }   // Boy (cm)
        public double? WeightKg { get; set; }   // Kilo (kg)

        [StringLength(20)]
        public string? Gender { get; set; }     // İstersen "Kadın/Erkek/Diğer"

        [StringLength(200)]
        public string? FitnessGoal { get; set; } // Örn: Kilo Verme, Kas Geliştirme

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
