using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GymApp.Controllers
{
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Services
        public async Task<IActionResult> Index()
        {
            var services = await _context.Services
                .Include(s => s.Gym)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View(services);
        }

        // GET: /Services/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var service = await _context.Services
                .Include(s => s.Gym)
                .Include(s => s.TrainerServices)
                    .ThenInclude(ts => ts.Trainer)
                        .ThenInclude(t => t.TrainerSpecialization)  
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null)
                return NotFound();

            return View(service);
        }

    }
}
