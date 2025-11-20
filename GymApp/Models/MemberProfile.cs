using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace GymApp.Models
{
    public class MemberProfile
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;  // ApplicationUser FK
        public ApplicationUser User { get; set; } = null!;

        [Required, StringLength(100)]
        public string FullName { get; set; } = null!;

        public int? HeightCm { get; set; }
        public double? WeightKg { get; set; }
        public string? Goal { get; set; }            // Kilo verme, kas yapma...

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
