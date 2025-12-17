using GymApp.Models.ViewModels;
using GymApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace GymApp.Controllers
{
    [Authorize]
    public class AICenterController : Controller
    {
        private readonly AiService _ai;

        public AICenterController(AiService ai)
        {
            _ai = ai;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var vm = new AICenterViewModel
            {
                Request = new AiService.AiRequestDto
                {
                    Goal = "Kilo Verme",
                    Age = 20,
                    HeightCm = 165,
                    WeightKg = 55,
                    Level = "Başlangıç",
                    DaysPerWeek = 3,
                    Equipment = "Ekipmansız"
                }
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(AICenterViewModel vm, CancellationToken ct)
        {
            if (vm.Request == null)
                vm.Request = new AiService.AiRequestDto();

            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                vm.Result = await _ai.GetRecommendationAsync(vm.Request, ct);
                vm.Error = null;

                return View("Result", vm);
            }
            catch (Exception ex)
            {
                vm.Error = ex.Message;
                vm.Result = null;
                return View(vm);
            }
        }
    }
}
