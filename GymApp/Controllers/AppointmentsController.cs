using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Controllers
{
    [Authorize] // Sadece giriş yapmış kullanıcılar randevu alabilsin
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentsController(ApplicationDbContext context,
                                      UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Appointments
        // Kullanıcının kendi randevularını listeler
        public async Task<IActionResult> Index(string status)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge(); // tekrar login iste

            var member = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.ApplicationUserId == user.Id);

            if (member == null)
            {
                // Henüz profil oluşturmamışsa boş liste göster
                return View(Enumerable.Empty<Appointment>());
            }

            var query = _context.Appointments
                .Include(a => a.Service).ThenInclude(s => s.Gym)
                .Include(a => a.Trainer)
                .Where(a => a.MemberProfileId == member.Id)
                .AsQueryable();

            // Duruma göre filtreleme
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                query = query.Where(a => a.Status == status);
            }

            var appointments = await query
                .OrderBy(a => a.StartTime)
                .ToListAsync();

            ViewBag.SelectedStatus = status;

            return View(appointments);
        }

        // GET: /Appointments/Create?serviceId=5
        public async Task<IActionResult> Create(int serviceId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            // Profil olmasa bile sayfa açılsın, 404 vermesin
            var member = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.ApplicationUserId == user.Id);

            var service = await _context.Services
                .Include(s => s.Gym)
                .Include(s => s.TrainerServices)
                    .ThenInclude(ts => ts.Trainer)
                .FirstOrDefaultAsync(s => s.Id == serviceId);

            if (service == null)
                return NotFound();

            var trainers = service.TrainerServices
                .Select(ts => ts.Trainer)
                .Where(t => t != null)
                .Distinct()
                .ToList();

            // Varsayılan olarak ilk antrenör
            int selectedTrainerId = trainers.Any() ? trainers.First().Id : 0;

            ViewBag.Service = service;
            ViewBag.Trainers = trainers;
            ViewBag.SelectedTrainerId = selectedTrainerId;

            var model = new Appointment
            {
                ServiceId = service.Id,
                TrainerId = selectedTrainerId,
                StartTime = DateTime.Now.AddDays(1) // varsayılan tarih
            };

            return View(model);
        }

        // POST: /Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            // Navigation property'leri ModelState doğrulamasından çıkar
            ModelState.Remove(nameof(Appointment.Service));
            ModelState.Remove(nameof(Appointment.Trainer));
            ModelState.Remove(nameof(Appointment.MemberProfile));

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var member = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.ApplicationUserId == user.Id);

            // MemberProfile yoksa otomatik oluştur
            if (member == null)
            {
                member = new MemberProfile
                {
                    ApplicationUserId = user.Id
                    // Diğer alanlar varsayılan kalsın
                };
                _context.MemberProfiles.Add(member);
                await _context.SaveChangesAsync();
            }

            // Hizmeti mutlaka çekmemiz lazım (süre ve fiyat için)
            var service = await _context.Services
                .Include(s => s.Gym)
                .Include(s => s.TrainerServices)
                    .ThenInclude(ts => ts.Trainer)
                .FirstOrDefaultAsync(s => s.Id == appointment.ServiceId);

            if (service == null)
            {
                ModelState.AddModelError(string.Empty, "Seçilen hizmet bulunamadı.");
            }

            if (member != null && service != null)
            {
                // Bu randevuyu giriş yapan üyeye bağla
                appointment.MemberProfileId = member.Id;

                // Süre ve ücreti o andaki hizmetten kopyala
                appointment.DurationMinutes = service.DurationMinutes;
                appointment.Price = service.Price;
                appointment.Status = "Pending";

                // 1) Geçmişe randevu alınamaz
                if (appointment.StartTime <= DateTime.Now)
                {
                    ModelState.AddModelError("StartTime", "Geçmiş bir zamana randevu alamazsınız.");
                }

                // Yeni randevunun bitiş zamanı
                var newStart = appointment.StartTime;
                var newEnd = appointment.StartTime.AddMinutes(appointment.DurationMinutes);

                // 1.5) Antrenörün günlük çalışma saatleri içinde mi? (AvailableFrom / AvailableTo)
                var trainer = await _context.Trainers
                    .FirstOrDefaultAsync(t => t.Id == appointment.TrainerId);

                if (trainer != null && trainer.AvailableFrom.HasValue && trainer.AvailableTo.HasValue)
                {
                    var appStartTimeOfDay = appointment.StartTime.TimeOfDay;
                    var appEndTimeOfDay = appointment.StartTime
                        .AddMinutes(appointment.DurationMinutes)
                        .TimeOfDay;

                    var trainerStart = trainer.AvailableFrom.Value;
                    var trainerEnd = trainer.AvailableTo.Value;

                    if (!(appStartTimeOfDay >= trainerStart && appEndTimeOfDay <= trainerEnd))
                    {
                        ModelState.AddModelError("StartTime", "Antrenör bu saat aralığında çalışmamaktadır.");
                    }
                }

                // 2) Aynı antrenörün başka randevusu ile çakışma var mı?
                var trainerConflict = await _context.Appointments
                    .Where(a => a.TrainerId == appointment.TrainerId &&
                                a.Status != "Cancelled" &&
                                a.Status != "Rejected")
                    .AnyAsync(a =>
                        newStart < a.StartTime.AddMinutes(a.DurationMinutes) &&
                        newEnd > a.StartTime);

                if (trainerConflict)
                {
                    ModelState.AddModelError("StartTime", "Bu antrenör seçtiğiniz saatte başka bir randevuya sahip.");
                }

                // 3) Üyenin kendi randevularıyla çakışma var mı?
                var memberConflict = await _context.Appointments
                    .Where(a => a.MemberProfileId == member.Id &&
                                a.Status != "Cancelled" &&
                                a.Status != "Rejected")
                    .AnyAsync(a =>
                        newStart < a.StartTime.AddMinutes(a.DurationMinutes) &&
                        newEnd > a.StartTime);

                if (memberConflict)
                {
                    ModelState.AddModelError("StartTime", "Bu saat aralığında zaten bir randevunuz bulunuyor.");
                }
            }

            if (!ModelState.IsValid)
            {
                var trainers = service?.TrainerServices
                    .Select(ts => ts.Trainer)
                    .Where(t => t != null)
                    .Distinct()
                    .ToList() ?? new List<Trainer>();

                ViewBag.Service = service;
                ViewBag.Trainers = trainers;
                ViewBag.SelectedTrainerId = appointment.TrainerId;

                return View(appointment);
            }

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Appointments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var member = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.ApplicationUserId == user.Id);

            if (member == null)
            {
                TempData["ErrorMessage"] = "Önce üye profili oluşturmanız gerekiyor.";
                return RedirectToAction(nameof(Index));
            }

            var appointment = await _context.Appointments
                .Include(a => a.Service)
                    .ThenInclude(s => s.Gym)
                .Include(a => a.Service)
                    .ThenInclude(s => s.TrainerServices)
                        .ThenInclude(ts => ts.Trainer)
                .FirstOrDefaultAsync(a => a.Id == id && a.MemberProfileId == member.Id);

            if (appointment == null)
                return NotFound();

            if (appointment.StartTime <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "Geçmiş randevular düzenlenemez.";
                return RedirectToAction(nameof(Index));
            }

            var service = appointment.Service;
            var trainers = service.TrainerServices
                .Select(ts => ts.Trainer)
                .Where(t => t != null)
                .Distinct()
                .ToList();

            ViewBag.Service = service;
            ViewBag.Trainers = trainers;
            ViewBag.SelectedTrainerId = appointment.TrainerId;

            return View(appointment);
        }

        // POST: /Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Appointment formAppointment)
        {
            if (id != formAppointment.Id)
                return NotFound();

            ModelState.Remove(nameof(Appointment.Service));
            ModelState.Remove(nameof(Appointment.Trainer));
            ModelState.Remove(nameof(Appointment.MemberProfile));

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var member = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.ApplicationUserId == user.Id);

            if (member == null)
            {
                ModelState.AddModelError(string.Empty, "Önce profil oluşturmanız gerekiyor.");
            }

            var existing = await _context.Appointments
                .Include(a => a.Service)
                    .ThenInclude(s => s.Gym)
                .Include(a => a.Service)
                    .ThenInclude(s => s.TrainerServices)
                        .ThenInclude(ts => ts.Trainer)
                .FirstOrDefaultAsync(a => a.Id == id && a.MemberProfileId == member.Id);

            if (existing == null)
                return NotFound();

            var service = existing.Service;

            if (member != null && service != null)
            {
                if (existing.StartTime <= DateTime.Now)
                {
                    ModelState.AddModelError(string.Empty, "Geçmiş randevular düzenlenemez.");
                }

                // Kullanıcının seçtiği yeni değerleri uygula
                existing.TrainerId = formAppointment.TrainerId;
                existing.StartTime = formAppointment.StartTime;

                // Süre ve ücreti servisten tekrar kopyala (değişmiş olabilir)
                existing.DurationMinutes = service.DurationMinutes;
                existing.Price = service.Price;

                // Değişiklikten sonra tekrar onaya düşsün
                existing.Status = "Pending";

                var newStart = existing.StartTime;
                var newEnd = existing.StartTime.AddMinutes(existing.DurationMinutes);

                // 1.5) Antrenör çalışma saatleri kontrolü (AvailableFrom / AvailableTo)
                var trainer = await _context.Trainers
                    .FirstOrDefaultAsync(t => t.Id == existing.TrainerId);

                if (trainer != null && trainer.AvailableFrom.HasValue && trainer.AvailableTo.HasValue)
                {
                    var appStartTimeOfDay = existing.StartTime.TimeOfDay;
                    var appEndTimeOfDay = existing.StartTime
                        .AddMinutes(existing.DurationMinutes)
                        .TimeOfDay;

                    var trainerStart = trainer.AvailableFrom.Value;
                    var trainerEnd = trainer.AvailableTo.Value;

                    if (!(appStartTimeOfDay >= trainerStart && appEndTimeOfDay <= trainerEnd))
                    {
                        ModelState.AddModelError("StartTime", "Antrenör bu saat aralığında çalışmamaktadır.");
                    }
                }

                // Aynı antrenörün başka randevusu var mı? (kendisi hariç)
                var trainerConflict = await _context.Appointments
                    .Where(a => a.TrainerId == existing.TrainerId &&
                                a.Id != existing.Id &&
                                a.Status != "Cancelled" &&
                                a.Status != "Rejected")
                    .AnyAsync(a =>
                        newStart < a.StartTime.AddMinutes(a.DurationMinutes) &&
                        newEnd > a.StartTime);

                if (trainerConflict)
                {
                    ModelState.AddModelError("StartTime", "Bu antrenör seçtiğiniz saatte başka bir randevuya sahip.");
                }

                // Üyenin kendi randevuları ile çakışma var mı? (kendisi hariç)
                var memberConflict = await _context.Appointments
                    .Where(a => a.MemberProfileId == member.Id &&
                                a.Id != existing.Id &&
                                a.Status != "Cancelled" &&
                                a.Status != "Rejected")
                    .AnyAsync(a =>
                        newStart < a.StartTime.AddMinutes(a.DurationMinutes) &&
                        newEnd > a.StartTime);

                if (memberConflict)
                {
                    ModelState.AddModelError("StartTime", "Bu saat aralığında zaten başka bir randevunuz bulunuyor.");
                }
            }

            if (!ModelState.IsValid)
            {
                var trainers = service?.TrainerServices
                    .Select(ts => ts.Trainer)
                    .Where(t => t != null)
                    .Distinct()
                    .ToList() ?? new List<Trainer>();

                ViewBag.Service = service;
                ViewBag.Trainers = trainers;
                ViewBag.SelectedTrainerId = existing.TrainerId;

                return View(existing);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: /Appointments/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var member = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.ApplicationUserId == user.Id);

            if (member == null)
                return RedirectToAction(nameof(Index));

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.MemberProfileId == member.Id);

            if (appointment == null)
                return NotFound();

            if (appointment.StartTime <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "Geçmiş randevular iptal edilemez.";
                return RedirectToAction(nameof(Index));
            }

            appointment.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
