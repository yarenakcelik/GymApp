using System;
using System.Threading.Tasks;
using GymApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace GymApp.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1) Rolleri oluştur (yoksa)
            string[] roles = { "Admin", "Member" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2) Admin kullanıcıyı oluştur
            // 👉 Burayı kendi numarana göre DÜZENLE
            string adminEmail = "B231210087@sakarya.edu.tr";   // ÖRN:  B231210123@sakarya.edu.tr
            string adminPassword = "sau";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);

                if (!createResult.Succeeded)
                {
                    // Hata olursa debug kolay olsun diye buraya breakpoint koyabilirsin
                    throw new Exception("Admin kullanıcısı oluşturulamadı: " +
                        string.Join(" | ", createResult.Errors));
                }
            }

            // 3) Admin rolüne ekle
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // 4) Örnek Trainer Specialization'lar
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (!context.TrainerSpecializations.Any())
            {
                context.TrainerSpecializations.AddRange(
                    new TrainerSpecialization { Name = "Kas Geliştirme", Description = "Hipertrofi odaklı antrenman" },
                    new TrainerSpecialization { Name = "Kilo Verme", Description = "Yağ yakım odaklı programlar" },
                    new TrainerSpecialization { Name = "Fonksiyonel Antrenman", Description = "Mobilite ve güç odaklı" },
                    new TrainerSpecialization { Name = "Yoga/Pilates", Description = "Esneklik ve denge" }
                );

                await context.SaveChangesAsync();
            }

        }
    }
}
