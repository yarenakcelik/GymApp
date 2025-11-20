using System.ComponentModel.DataAnnotations;
namespace GymApp.Models
{
    public class TrainerSpecialization
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = null!;   // Kas Geliştirme, Yoga...

        public int TrainerId { get; set; }
        public Trainer Trainer { get; set; } = null!;
    }
}
