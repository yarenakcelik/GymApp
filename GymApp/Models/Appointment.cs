using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace GymApp.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        public int MemberProfileId { get; set; }
        public MemberProfile MemberProfile { get; set; } = null!;

        public int TrainerId { get; set; }
        public Trainer Trainer { get; set; } = null!;

        public int ServiceId { get; set; }
        public Service Service { get; set; } = null!;

        public DateTime StartTime { get; set; }      // 18 Kasım 14:00
        public DateTime EndTime { get; set; }        // Start + Duration

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    }

    public enum AppointmentStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Cancelled = 3
    }
}
