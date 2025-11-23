using GymApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Gym> Gyms { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<TrainerSpecialization> TrainerSpecializations { get; set; }
        public DbSet<TrainerService> TrainerServices { get; set; }
        public DbSet<MemberProfile> MemberProfiles { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ---- TrainerService (N-N) ----
            builder.Entity<TrainerService>()
                .HasKey(ts => new { ts.TrainerId, ts.ServiceId });

            builder.Entity<TrainerService>()
                .HasOne(ts => ts.Trainer)
                .WithMany(t => t.TrainerServices) // Trainer modelinde ICollection<TrainerService> varsa
                .HasForeignKey(ts => ts.TrainerId)
                .OnDelete(DeleteBehavior.NoAction); // CASCADE KIR

            builder.Entity<TrainerService>()
                .HasOne(ts => ts.Service)
                .WithMany(s => s.TrainerServices)   // Service modelinde ICollection<TrainerService> varsa
                .HasForeignKey(ts => ts.ServiceId)
                .OnDelete(DeleteBehavior.NoAction); // İstersen burada da NoAction

            // ---- Appointment ilişkileri ----
            builder.Entity<Appointment>()
                .HasOne(a => a.Trainer)
                .WithMany(t => t.Appointments)
                .HasForeignKey(a => a.TrainerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Appointment>()
                .HasOne(a => a.Service)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Appointment>()
                .HasOne(a => a.MemberProfile)
                .WithMany(m => m.Appointments)
                .HasForeignKey(a => a.MemberProfileId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
