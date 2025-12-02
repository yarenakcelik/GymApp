using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class TrainerSpecialization
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Uzmanlık Adı")]
        public string Name { get; set; } = null!;   // Örn: Kas Geliştirme, Kilo Verme, Yoga

        [StringLength(250)]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        public ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();
    }
}
