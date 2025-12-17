using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class MemberProfile
    {
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser ApplicationUser { get; set; } = null!;

        [StringLength(100)]
        public string? FullName { get; set; }

        public double? HeightCm { get; set; }   
        public double? WeightKg { get; set; }   

        [StringLength(20)]
        public string? Gender { get; set; }    

        [StringLength(200)]
        public string? FitnessGoal { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
