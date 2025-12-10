using System;
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
        public async Task<IActionResult> Index()
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

            var appointments = await _context.Appointments
                .Include(a => a.Service).ThenInclude(s => s.Gym)
                .Include(a => a.Trainer)
                .Where(a => a.MemberProfileId == member.Id)
                .OrderBy(a => a.StartTime)
                .ToListAsync();

            return View(appointments);
        }

        // GET: /Appointments/Create?serviceId=5
        public async Task<IActionResult> Create(int serviceId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var member = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.ApplicationUserId == user.Id);

            if (member == null)
                return RedirectToAction("Create", "MemberProfiles");

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

            if (member == null)
            {
                ModelState.AddModelError(string.Empty, "Önce profil oluşturmanız gerekiyor.");
            }

            if (!ModelState.IsValid)
            {
                var service = await _context.Services
                    .Include(s => s.Gym)
                    .Include(s => s.TrainerServices)
                        .ThenInclude(ts => ts.Trainer)
                    .FirstOrDefaultAsync(s => s.Id == appointment.ServiceId);

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

            // Bu randevuyu giriş yapan üyeye bağla
            appointment.MemberProfileId = member.Id;

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
