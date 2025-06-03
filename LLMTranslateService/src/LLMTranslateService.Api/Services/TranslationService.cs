using LLMTranslateService.Api.Models;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace LLMTranslateService.Api.Services
{
    public interface ITranslationService
    {
        TranslationResponse Translate(TranslationRequest request);
    }

    public class TranslationService : ITranslationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _endpointUrl;

        public TranslationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _endpointUrl = configuration["TranslationService:Endpoint"] ?? throw new ArgumentNullException("TranslationService:Endpoint configuration is missing.");
        }

        public TranslationResponse Translate(TranslationRequest request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = _httpClient.PostAsync(_endpointUrl, content).Result;
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
