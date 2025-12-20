using GymApp.Models.ViewModels;
using GymApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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

        //FOTOĞRAF ANALİZİ
        // POST: /AICenter/AnalyzeMeal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnalyzeMeal(IFormFile photo, CancellationToken ct)
        {
            if (photo == null || photo.Length == 0)
                return BadRequest(new { ok = false, message = "Lütfen bir fotoğraf yükleyin." });

            if (!photo.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { ok = false, message = "Sadece resim dosyası yükleyebilirsiniz." });

            // Groq base64 image limitleri var; güvenli olsun diye 4MB yapalım
            const long maxBytes = 4 * 1024 * 1024;
            if (photo.Length > maxBytes)
                return BadRequest(new { ok = false, message = "Dosya çok büyük. En fazla 4MB yükleyin." });

            await using var ms = new MemoryStream();
            await photo.CopyToAsync(ms, ct);
            var bytes = ms.ToArray();

            try
            {
                var result = await _ai.AnalyzeMealPhotoAsync(bytes, photo.ContentType, ct);

                if (!result.HasPlate)
                {
                    return Ok(new
                    {
                        ok = true,
                        hasPlate = false,
                        message = "Tabak/yemek bulunamadı. Lütfen net bir yemek fotoğrafı yükleyin."
                    });
                }

                return Ok(new
                {
                    ok = true,
                    hasPlate = true,
                    data = new
                    {
                        mealName = result.MealName,
                        estimatedGrams = result.EstimatedGrams,
                        caloriesKcal = result.CaloriesKcal,
                        proteinG = result.ProteinG,
                        carbsG = result.CarbsG,
                        fatG = result.FatG,
                        notes = result.Notes
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, message = ex.Message });
            }
        }
    }
}
