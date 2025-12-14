using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Controllers.Api
{
    [ApiController]
    [Route("api/appointments")]
    [Authorize]
    public class AppointmentsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentsApiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /api/appointments/my?status=Onaylandi
        [HttpGet("my")]
        public async Task<IActionResult> My([FromQuery] string? status)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var member = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.ApplicationUserId == user.Id);

            if (member == null) return Ok(new List<object>());

            var query = _context.Appointments
                .Include(a => a.Service).ThenInclude(s => s.Gym)
                .Include(a => a.Trainer)
                .Where(a => a.MemberProfileId == member.Id)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status != "Hepsi")
            {
                // Status değerlerin senin sistemde: Pending/Approved/Rejected/Cancelled
                // ama ekranda Türkçe gösteriyorsun.
                // Bu yüzden burada Türkçe -> İngilizce map yapacağız:
                status = status.Trim();

                var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Onay Bekliyor", "Pending" },
                    { "Onaylandı", "Approved" },
                    { "Reddedildi", "Rejected" },
                    { "İptal Edildi", "Cancelled" },
                    { "Iptal Edildi", "Cancelled" } // olası yazım
                };

                if (map.ContainsKey(status))
                    query = query.Where(a => a.Status == map[status]);
                else
                    query = query.Where(a => a.Status == status); // direkt gelirse
            }

            var data = await query
                .OrderBy(a => a.StartTime)
                .Select(a => new
                {
                    a.Id,
                    ServiceName = a.Service != null ? a.Service.Name : "-",
                    GymName = (a.Service != null && a.Service.Gym != null) ? a.Service.Gym.Name : "-",
                    TrainerName = a.Trainer != null ? a.Trainer.FullName : "-",
                    StartTime = a.StartTime,
                    StartTimeText = a.StartTime.ToString("g"),
                    a.DurationMinutes,
                    PriceText = a.Price.ToString("N2") + " ₺",
                    Status = a.Status,
                    StatusText = a.Status == "Pending" ? "Onay Bekliyor"
                               : a.Status == "Approved" ? "Onaylandı"
                               : a.Status == "Rejected" ? "Reddedildi"
                               : a.Status == "Cancelled" ? "İptal Edildi"
                               : a.Status
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
