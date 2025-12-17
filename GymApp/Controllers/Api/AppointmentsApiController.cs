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
           
                status = status.Trim();

                var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Onay Bekliyor", "Pending" },
                    { "Onaylandı", "Approved" },
                    { "Reddedildi", "Rejected" },
                    { "İptal Edildi", "Cancelled" },
                    { "Iptal Edildi", "Cancelled" } 
                };

                if (map.ContainsKey(status))
                    query = query.Where(a => a.Status == map[status]);
                else
                    query = query.Where(a => a.Status == status);
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

        // GET: /api/appointments/slots?trainerId=1&serviceId=5&date=2025-12-18
        [HttpGet("slots")]
        public async Task<IActionResult> Slots([FromQuery] int trainerId, [FromQuery] int serviceId, [FromQuery] string date)
        {

            if (!DateTime.TryParse(date, out var day))
                return BadRequest("Invalid date");

            var trainer = await _context.Trainers.FirstOrDefaultAsync(t => t.Id == trainerId);
            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == serviceId);

            if (trainer == null || service == null)
                return NotFound();

            if (!trainer.AvailableFrom.HasValue || !trainer.AvailableTo.HasValue)
                return Ok(new List<SlotResponse>());

            var workStart = day.Date.Add(trainer.AvailableFrom.Value);
            var workEnd = day.Date.Add(trainer.AvailableTo.Value);

            var duration = service.DurationMinutes;
            if (duration <= 0) return Ok(new List<SlotResponse>());

            var apps = await _context.Appointments
                .Where(a => a.TrainerId == trainerId &&
                            a.Status != "Cancelled" &&
                            a.Status != "Rejected" &&
                            a.StartTime.Date == day.Date)
                .Select(a => new { a.StartTime, a.DurationMinutes })
                .ToListAsync();

            bool Conflicts(DateTime start, DateTime end)
            {
                foreach (var a in apps)
                {
                    var aStart = a.StartTime;
                    var aEnd = a.StartTime.AddMinutes(a.DurationMinutes);
                    if (start < aEnd && end > aStart) return true;
                }
                return false;
            }

            var stepMinutes = 30;

            var slots = new List<SlotResponse>();
            for (var t = workStart; t.AddMinutes(duration) <= workEnd; t = t.AddMinutes(stepMinutes))
            {
                if (t <= DateTime.Now) continue;

                var end = t.AddMinutes(duration);
                if (Conflicts(t, end)) continue;

                slots.Add(new SlotResponse
                {
                    StartIso = t.ToString("yyyy-MM-ddTHH:mm"),
                    Label = t.ToString("HH:mm")
                });
            }

            return Ok(slots);
        }


    }

    public class SlotResponse
    {
        public string StartIso { get; set; } = ""; 
        public string Label { get; set; } = "";    
    }


}
