using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace GymApp.Models
{
    public class Gym
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = null!;   // PowerFit, EliteGym...

        [StringLength(200)]
        public string? Address { get; set; }

        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }

        public ICollection<Service> Services { get; set; } = new List<Service>();
        public ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();
    }
}
