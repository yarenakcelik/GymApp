using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class Trainer
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; } = null!;

        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }   // Kısa tanıtım

        // Hangi salonda çalışıyor?
        public int GymId { get; set; }
        public Gym Gym { get; set; } = null!;

        // Uzmanlık alanı (tek bir ana uzmanlık)
        public int? TrainerSpecializationId { get; set; }
        public TrainerSpecialization? TrainerSpecialization { get; set; }

        // Müsait olduğu saat aralığı (basit versiyon)
        public TimeSpan? AvailableFrom { get; set; }   // Örn: 09:00
        public TimeSpan? AvailableTo { get; set; }     // Örn: 18:00

        // İlişkiler
        public ICollection<TrainerService> TrainerServices { get; set; } = new List<TrainerService>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
