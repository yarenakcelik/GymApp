using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GymApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TrainersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Trainers
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Trainers
                .Include(t => t.Gym)
                .Include(t => t.TrainerSpecialization);

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Admin/Trainers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers
                .Include(t => t.Gym)
                .Include(t => t.TrainerSpecialization)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trainer == null)
            {
                return NotFound();
            }

            return View(trainer);
        }

        // GET: Admin/Trainers/Create
        public IActionResult Create()
        {
            ViewData["GymId"] = new SelectList(_context.Gyms, "Id", "Name");
            ViewData["TrainerSpecializationId"] = new SelectList(_context.TrainerSpecializations, "Id", "Name");
            ViewBag.Services = _context.Services.ToList(); 
            return View();
        }

        // POST: Admin/Trainers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,FullName,Email,Phone,Bio,GymId,TrainerSpecializationId,AvailableFrom,AvailableTo")] Trainer trainer,
            int[] selectedServiceIds)   
        {
            // En az bir hizmet seçildi mi kontrol etme
            if (selectedServiceIds == null || selectedServiceIds.Length == 0)
            {
                ModelState.AddModelError("SelectedServiceIds", "En az bir hizmet seçmelisiniz.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(trainer);
                await _context.SaveChangesAsync();

                //Seçili hizmetler için TrainerService kayıtları oluşturma
                foreach (var serviceId in selectedServiceIds)
                {
                    _context.TrainerServices.Add(new TrainerService
                    {
                        TrainerId = trainer.Id,
                        ServiceId = serviceId
                    });
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // ModelState geçersizse dropdown ve hizmet listesini tekrar doldurma
            ViewData["GymId"] = new SelectList(_context.Gyms, "Id", "Name", trainer.GymId);
            ViewData["TrainerSpecializationId"] = new SelectList(_context.TrainerSpecializations, "Id", "Name", trainer.TrainerSpecializationId);
            ViewBag.Services = _context.Services.ToList();

            return View(trainer);
        }


        // GET: Admin/Trainers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer == null)
            {
                return NotFound();
            }

            ViewData["GymId"] = new SelectList(_context.Gyms, "Id", "Name", trainer.GymId);
            ViewData["TrainerSpecializationId"] = new SelectList(_context.TrainerSpecializations, "Id", "Name", trainer.TrainerSpecializationId);
            return View(trainer);
        }

        // POST: Admin/Trainers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Email,Phone,Bio,GymId,TrainerSpecializationId,AvailableFrom,AvailableTo")] Trainer trainer)
        {
            if (id != trainer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trainer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainerExists(trainer.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["GymId"] = new SelectList(_context.Gyms, "Id", "Name", trainer.GymId);
            ViewData["TrainerSpecializationId"] = new SelectList(_context.TrainerSpecializations, "Id", "Name", trainer.TrainerSpecializationId);
            return View(trainer);
        }

        // GET: Admin/Trainers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers
                .Include(t => t.Gym)
                .Include(t => t.TrainerSpecialization)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trainer == null)
            {
                return NotFound();
            }

            return View(trainer);
        }

        // POST: Admin/Trainers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer != null)
            {
                _context.Trainers.Remove(trainer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Trainers/ManageServices/5
        public async Task<IActionResult> ManageServices(int id)
        {
            var trainer = await _context.Trainers
                .Include(t => t.TrainerServices)
                    .ThenInclude(ts => ts.Service)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trainer == null)
                return NotFound();

            var allServices = await _context.Services.ToListAsync();
            var selectedIds = trainer.TrainerServices
                .Select(ts => ts.ServiceId)
                .ToHashSet();

            var viewModel = new TrainerServicesViewModel
            {
                TrainerId = trainer.Id,
                TrainerName = trainer.FullName,
                Services = allServices.Select(s => new ServiceCheckboxItem
                {
                    ServiceId = s.Id,
                    Name = s.Name,
                    IsSelected = selectedIds.Contains(s.Id)
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Admin/Trainers/ManageServices
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageServices(TrainerServicesViewModel model)
        {
            var trainer = await _context.Trainers
                .Include(t => t.TrainerServices)
                .FirstOrDefaultAsync(t => t.Id == model.TrainerId);

            if (trainer == null)
                return NotFound();

            // Eski eşleştirmeleri silme
            _context.TrainerServices.RemoveRange(trainer.TrainerServices);

            // Yeni seçilen hizmetler için kayıt ekleme
            if (model.SelectedServiceIds != null)
            {
                foreach (var serviceId in model.SelectedServiceIds)
                {
                    _context.TrainerServices.Add(new TrainerService
                    {
                        TrainerId = trainer.Id,
                        ServiceId = serviceId
                    });
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool TrainerExists(int id)
        {
            return _context.Trainers.Any(e => e.Id == id);
        }
    }
}
