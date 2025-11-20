using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GymApp.Models
{
     public class Service
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = null!;   // Yoga, Kişisel Antrenman...

        public int DurationMinutes { get; set; }    // 60 dk

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }          // Ücret

        // Hangi salona ait?
        public int GymId { get; set; }
        public Gym Gym { get; set; } = null!;

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<TrainerService> TrainerServices { get; set; } = new List<TrainerService>();
    }
}
