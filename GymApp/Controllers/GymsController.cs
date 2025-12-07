using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GymApp.Controllers
{
    public class GymsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GymsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Gyms
        // Müşteri tarafı spor salonu listesi
        public async Task<IActionResult> Index()
        {
            var gyms = await _context.Gyms
                .OrderBy(g => g.Name)
                .ToListAsync();

            return View(gyms);
        }

        // GET: /Gyms/Details/5
        // Tek spor salonu detay sayfası
        public async Task<IActionResult> Details(int id)
        {
            var gym = await _context.Gyms
                .Include(g => g.Services)          // Gym -> Service (1-N) varsayıyorum
                .Include(g => g.Trainers)          // Gym -> Trainer (1-N)
                    .ThenInclude(t => t.TrainerSpecialization)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gym == null)
                return NotFound();

            return View(gym);
        }
    }
}
