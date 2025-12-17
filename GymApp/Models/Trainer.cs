using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class Trainer
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = null!;

        [StringLength(200)]
        [EmailAddress]
        [Display(Name = "E-posta")]
        public string? Email { get; set; }

        [StringLength(20)]
        [Phone]
        [Display(Name = "Telefon")]
        public string? Phone { get; set; }

        [StringLength(500)]
        [Display(Name = "Biyografi")]
        public string? Bio { get; set; }

        [Required(ErrorMessage = "Lütfen bir spor salonu seçin.")]
        [Display(Name = "Spor Salonu")]
        public int GymId { get; set; }

        public Gym? Gym { get; set; }

        [Display(Name = "Uzmanlık Alanı")]
        public int? TrainerSpecializationId { get; set; }
        public TrainerSpecialization? TrainerSpecialization { get; set; }

        [Display(Name = "Müsaitlik Başlangıcı")]
        public TimeSpan? AvailableFrom { get; set; }

        [Display(Name = "Müsaitlik Bitişi")]
        public TimeSpan? AvailableTo { get; set; }

        public ICollection<TrainerService> TrainerServices { get; set; } = new List<TrainerService>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
