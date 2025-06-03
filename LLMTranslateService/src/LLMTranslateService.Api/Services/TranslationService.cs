using LLMTranslateService.Api.Models;
using System.Text;
using System.Text.Json;

namespace LLMTranslateService.Api.Services
{
    public interface ITranslationService
    {
        TranslationResponse Translate(TranslationRequest request);
    }

    public class TranslationService : ITranslationService
    {
        private readonly HttpClient _httpClient;

        public TranslationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public TranslationResponse Translate(TranslationRequest request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = _httpClient.PostAsync("http://localhost:8000/generate", content).Result;
            var responseBody = response.Content.ReadAsStringAsync().Result;

            List<string>? translatedStrings = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(responseBody) && responseBody.TrimStart().StartsWith("["))
                {
                    translatedStrings = JsonSerializer.Deserialize<List<string>>(responseBody);
                }
            }
            catch (JsonException)
            {
                translatedStrings = new List<string>();
            }

            var messages = translatedStrings != null
                ? translatedStrings.Select(msg => new TranslationMessage { Message = msg }).ToList()
                : new List<TranslationMessage>();

            return new TranslationResponse
            {
                Language = request.Language,
                Messages = messages
            };
        }
    }
}
