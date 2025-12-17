using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymApp.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        // Hangi üyenin randevusu?
        public int MemberProfileId { get; set; }
        public MemberProfile MemberProfile { get; set; } = null!;

        // Hangi eğitmen?
        public int TrainerId { get; set; }
        public Trainer Trainer { get; set; } = null!;

        // Hangi hizmet (PT, Yoga, Pilates vs.)
        public int ServiceId { get; set; }
        public Service Service { get; set; } = null!;

        // Randevu başlama zamanı
        [Required]
        public DateTime StartTime { get; set; }

        // Süre (dakika)
        [Range(0, 600)]
        public int DurationMinutes { get; set; }

        // O anki fiyat (ileride fiyat değişse bile randevuda sabit kalır)
        [Range(0, 100000)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }


        // Onay durumu
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
