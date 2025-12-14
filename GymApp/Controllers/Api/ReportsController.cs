using GymApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // giriş yapan görsün (istersen admin şartı da koyarız)
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /api/reports/trainers
        // LINQ ile filtre: gymId, specializationId
        [HttpGet("trainers")]
        public async Task<IActionResult> GetTrainers([FromQuery] int? gymId, [FromQuery] int? specializationId)
        {
            var query = _context.Trainers
                .Include(t => t.Gym)
                .Include(t => t.TrainerSpecialization)
                .AsQueryable();

            if (gymId.HasValue)
                query = query.Where(t => t.GymId == gymId.Value);

            if (specializationId.HasValue)
                query = query.Where(t => t.TrainerSpecializationId == specializationId.Value);

            var result = await query
                .OrderBy(t => t.FullName)
                .Select(t => new
                {
                    t.Id,
                    t.FullName,
                    GymName = t.Gym != null ? t.Gym.Name : "-",
                    Specialization = t.TrainerSpecialization != null ? t.TrainerSpecialization.Name : "-",
                    AvailableFrom = t.AvailableFrom.HasValue ? t.AvailableFrom.Value.ToString(@"hh\:mm") : "-",
                    AvailableTo = t.AvailableTo.HasValue ? t.AvailableTo.Value.ToString(@"hh\:mm") : "-"
                })
                .ToListAsync();

            return Ok(result);
        }

        // GET: /api/reports/gyms
        [HttpGet("gyms")]
        public async Task<IActionResult> GetGyms()
        {
            var gyms = await _context.Gyms
                .OrderBy(g => g.Name)
                .Select(g => new { g.Id, g.Name })
                .ToListAsync();

            return Ok(gyms);
        }

        // GET: /api/reports/specializations
        [HttpGet("specializations")]
        public async Task<IActionResult> GetSpecializations()
        {
            var specs = await _context.TrainerSpecializations
                .OrderBy(s => s.Name)
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();

            return Ok(specs);
        }
    }
}
