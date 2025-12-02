using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GymApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TrainerSpecializationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainerSpecializationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/TrainerSpecializations
        public async Task<IActionResult> Index()
        {
            var list = await _context.TrainerSpecializations.ToListAsync();
            return View(list);
        }

        // GET: Admin/TrainerSpecializations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var specialization = await _context.TrainerSpecializations
                .FirstOrDefaultAsync(m => m.Id == id);

            if (specialization == null)
                return NotFound();

            return View(specialization);
        }

        // GET: Admin/TrainerSpecializations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/TrainerSpecializations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainerSpecialization specialization)
        {
            if (ModelState.IsValid)
            {
                _context.Add(specialization);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(specialization);
        }

        // GET: Admin/TrainerSpecializations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var specialization = await _context.TrainerSpecializations.FindAsync(id);
            if (specialization == null)
                return NotFound();

            return View(specialization);
        }

        // POST: Admin/TrainerSpecializations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TrainerSpecialization specialization)
        {
            if (id != specialization.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(specialization);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainerSpecializationExists(specialization.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(specialization);
        }

        // GET: Admin/TrainerSpecializations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var specialization = await _context.TrainerSpecializations
                .FirstOrDefaultAsync(m => m.Id == id);

            if (specialization == null)
                return NotFound();

            return View(specialization);
        }

        // POST: Admin/TrainerSpecializations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var specialization = await _context.TrainerSpecializations.FindAsync(id);
            if (specialization != null)
            {
                _context.TrainerSpecializations.Remove(specialization);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TrainerSpecializationExists(int id)
        {
            return _context.TrainerSpecializations.Any(e => e.Id == id);
        }
    }
}
