using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GymApp.Controllers
{
    public class TrainersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Trainers
        public async Task<IActionResult> Index(int? gymId, int? specializationId)
        {
            var query = _context.Trainers
                .Include(t => t.Gym)
                .Include(t => t.TrainerSpecialization)
                .Include(t => t.TrainerServices)
                    .ThenInclude(ts => ts.Service)
                .AsQueryable();

            if (gymId.HasValue)
            {
                query = query.Where(t => t.GymId == gymId.Value);
            }

            if (specializationId.HasValue)
            {
                query = query.Where(t => t.TrainerSpecializationId == specializationId.Value);
            }

            // Filtre dropdown’ları için
            ViewBag.Gyms = await _context.Gyms
                .OrderBy(g => g.Name)
                .ToListAsync();

            ViewBag.Specializations = await _context.TrainerSpecializations
                .OrderBy(s => s.Name)
                .ToListAsync();

            ViewBag.SelectedGymId = gymId;
            ViewBag.SelectedSpecializationId = specializationId;

            var trainers = await query
                .OrderBy(t => t.FullName)
                .ToListAsync();

            return View(trainers);
        }

        // GET: /Trainers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var trainer = await _context.Trainers
                .Include(t => t.Gym)
                .Include(t => t.TrainerSpecialization)
                .Include(t => t.TrainerServices)
                    .ThenInclude(ts => ts.Service)
                        .ThenInclude(s => s.Gym)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trainer == null)
                return NotFound();

            return View(trainer);
        }
    }
}
