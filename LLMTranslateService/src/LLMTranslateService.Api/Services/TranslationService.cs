using LLMTranslateService.Api.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

            var response = _httpClient.PostAsync("https://api.otroservicio.com/translate", content).Result;
            var responseBody = response.Content.ReadAsStringAsync().Result;

            var translatedResponse = JsonSerializer.Deserialize<TranslationResponse>(responseBody);

            return translatedResponse ?? new TranslationResponse
            {
                Language = request.Language,
                Messages = new List<TranslationMessage>()
            };
        }
    }
}
