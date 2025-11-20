using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class Trainer
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; } = null!;   // Mehmet Kaya

        public double? Rating { get; set; }             // 4.9
        public int ReviewCount { get; set; }            // 156

        [StringLength(200)]
        public string? Title { get; set; }              // "Kas Geliştirme & Kilo Verme" gibi

        public int GymId { get; set; }
        public Gym Gym { get; set; } = null!;

        public ICollection<TrainerSpecialization> Specializations { get; set; } = new List<TrainerSpecialization>();
        public ICollection<TrainerService> TrainerServices { get; set; } = new List<TrainerService>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
