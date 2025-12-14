using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymApp.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        public IActionResult Trainers()
        {
            return View();
        }
    }
}
