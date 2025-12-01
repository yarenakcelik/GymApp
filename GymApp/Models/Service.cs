using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GymApp.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Hizmet Adı")]
        public string Name { get; set; } = null!;

        [StringLength(300)]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [StringLength(50)]
        [Display(Name = "Kategori")]
        public string? Category { get; set; }

        [Range(1, 600)]
        [Display(Name = "Süre (dakika)")]
        public int DurationMinutes { get; set; }

        [Range(0, 100000)]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Ücret (₺)")]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Spor Salonu")]
        public int GymId { get; set; }

        // 🔥 ÖNEMLİ KISIM:
        // Bu navigation property'yi validasyondan tamamen çıkarıyoruz
        // ve nullable yapıyoruz ki "Gym zorunlu" hatası üretmesin.
        [ValidateNever]
        public Gym? Gym { get; set; }

        public ICollection<TrainerService> TrainerServices { get; set; } = new List<TrainerService>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
