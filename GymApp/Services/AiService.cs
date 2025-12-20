using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GymApp.Services
{
    public class AiService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public AiService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        private string GetApiKey()
        {
            var apiKey = _config["Ai:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY", EnvironmentVariableTarget.User)
                      ?? Environment.GetEnvironmentVariable("GROQ_API_KEY", EnvironmentVariableTarget.Machine)
                      ?? Environment.GetEnvironmentVariable("GROQ_API_KEY");
            }

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Groq API Key bulunamadı. Ai:ApiKey veya GROQ_API_KEY ekleyin.");

            return apiKey.Trim();
        }

        public class AiRequestDto
        {
            public string Goal { get; set; } = "Kilo Verme";
            public int Age { get; set; } = 20;
            public int HeightCm { get; set; } = 165;
            public int WeightKg { get; set; } = 55;
            public string Level { get; set; } = "Başlangıç";
            public int DaysPerWeek { get; set; } = 3;
            public string Equipment { get; set; } = "Ekipmansız";
        }

        private class ChatCompletionResponse
        {
            public Choice[]? choices { get; set; }
            public class Choice { public Message? message { get; set; } }
            public class Message { public string? content { get; set; } }
        }

        public async Task<string> GetRecommendationAsync(AiRequestDto req, CancellationToken ct = default)
        {
            var apiKey = GetApiKey();
            var baseUrl = _config["Ai:BaseUrl"];
            var model = _config["Ai:Model"];

            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("Ai:BaseUrl bulunamadı. appsettings.json içine Ai:BaseUrl ekleyin.");
            if (string.IsNullOrWhiteSpace(model))
                throw new InvalidOperationException("Ai:Model bulunamadı. appsettings.json içine Ai:Model ekleyin.");

            var url = $"{baseUrl.TrimEnd('/')}/chat/completions";

            var systemPrompt =
                "Sen GymApp içinde çalışan bir AI koçsun. Çıktın SADECE HTML olacak. " +
                "Markdown kullanma. Kod bloğu (``` ) kullanma. " +
                "Tıbbi teşhis/ilaç önerme. Güvenli ve uygulanabilir öneriler ver.";

            var userPrompt = BuildHtmlPrompt(req);

            var payload = new
            {
                model = model,
                temperature = 0.7,
                messages = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                }
            };

            var json = JsonSerializer.Serialize(payload);

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var resp = await _http.SendAsync(request, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException($"AI API hata: {(int)resp.StatusCode} {resp.ReasonPhrase} - {body}");

            var parsed = JsonSerializer.Deserialize<ChatCompletionResponse>(body);
            var content = parsed?.choices?.FirstOrDefault()?.message?.content;

            return string.IsNullOrWhiteSpace(content) ? "AI boş yanıt döndürdü." : content.Trim();
        }

        public class MealAnalysisResult
        {
            public bool HasPlate { get; set; }
            public string MealName { get; set; } = "-";
            public int EstimatedGrams { get; set; }
            public int CaloriesKcal { get; set; }
            public int ProteinG { get; set; }
            public int CarbsG { get; set; }
            public int FatG { get; set; }
            public string Notes { get; set; } = "";
        }

        
        public async Task<MealAnalysisResult> AnalyzeMealPhotoAsync(
            byte[] imageBytes,
            string contentType,
            CancellationToken ct = default)
        {
            var apiKey = GetApiKey();
            var baseUrl = _config["Ai:BaseUrl"];
            var visionModel = _config["Ai:VisionModel"];

            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("Ai:BaseUrl bulunamadı.");
            if (string.IsNullOrWhiteSpace(visionModel))
                throw new InvalidOperationException("Ai:VisionModel tanımlı değil. appsettings.json içine ekleyin.");

            var url = $"{baseUrl.TrimEnd('/')}/chat/completions";

            var base64 = Convert.ToBase64String(imageBytes);
            var dataUrl = $"data:{contentType};base64,{base64}";

            var systemPrompt =
                "Sen bir beslenme uzmanı AI'sın. " +
                "Fotoğrafta yemek/tabak yoksa hasPlate=false dön. " +
                "Varsa yaklaşık makro değerleri tahmin et. " +
                "Çıktı SADECE JSON olacak. Markdown yok. Başka hiçbir metin yok.";

            var userText =
@"Bu bir yemek fotoğrafı.

Kurallar:
- Fotoğrafta gerçekten gördüğün yemeğe göre mealName üret.
- Emin değilsen mealName = ""Bilinmiyor"" yaz.
- ""Kahvaltı/Akşam yemeği"" gibi öğün ismi UYDURMA.
- Görseldeki ana öğeleri 2-5 kelimeyle belirt (örn: ""tavuk pilav"", ""hamburger patates"", ""salata"", ""çorba"").


SADECE aşağıdaki JSON formatında cevap ver:

{
  ""hasPlate"": true,
  ""mealName"": ""..."",
  ""estimatedGrams"": 0,
  ""caloriesKcal"": 0,
  ""proteinG"": 0,
  ""carbsG"": 0,
  ""fatG"": 0,
  ""notes"": ""Tahmini değerlerdir.""
}";

            var payload = new
            {
                model = visionModel,
                temperature = 0.2,
                messages = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = userText },
                            new { type = "image_url", image_url = new { url = dataUrl } }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var resp = await _http.SendAsync(request, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException($"AI API hata: {(int)resp.StatusCode} {resp.ReasonPhrase} - {body}");

            var parsed = JsonSerializer.Deserialize<ChatCompletionResponse>(body);
            var content = parsed?.choices?.FirstOrDefault()?.message?.content;

            if (string.IsNullOrWhiteSpace(content))
                throw new InvalidOperationException("AI boş yanıt döndürdü.");

            // JSON parse
            try
            {
                var result = JsonSerializer.Deserialize<MealAnalysisResult>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result == null)
                    throw new InvalidOperationException("AI JSON parse edilemedi.");

                return result;
            }
            catch
            {
                throw new InvalidOperationException("AI beklenen JSON formatında yanıt vermedi.");
            }
        }

        private static string BuildHtmlPrompt(AiRequestDto req)
        {
            return
$@"
Aşağıdaki bilgilere göre antrenman + beslenme önerisi üret.

Kullanıcı:
- Hedef: {req.Goal}
- Yaş: {req.Age}
- Boy: {req.HeightCm} cm
- Kilo: {req.WeightKg} kg
- Seviye: {req.Level}
- Haftada gün: {req.DaysPerWeek}
- Ekipman: {req.Equipment}

ÇIKTI KURALLARI (çok önemli):
- SADECE HTML döndür.
- Markdown kullanma (** ### -) kullanma.
- Şu şablona birebir uy:

<div class='ai-result'>
  <h4>Özet</h4>
  <p>...</p>

  <h4>Antrenman Planı</h4>
  <div class='day'>
    <h5>Gün 1</h5>
    <ul>
      <li>Hareket - set/tekrar/süre</li>
    </ul>
  </div>
  <div class='day'>
    <h5>Gün 2</h5>
  </div>

  <h4>Beslenme Önerisi</h4>
  <ul>
    <li>...</li>
  </ul>

  <h4>Dikkat / Uyarı</h4>
  <ul>
    <li>...</li>
  </ul>
</div>

- 3 ila 5 gün plan ver.
- Çok uzun yazma, net ve uygulanabilir olsun.
";
        }
    }
}
