using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class TrainerSpecialization
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = null!;   // Örn: Kas Geliştirme, Kilo Verme, Yoga

        [StringLength(250)]
        public string? Description { get; set; }

        public ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();
    }
}
