using LLMTranslateService.Api.Models;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;

namespace LLMTranslateService.Api.Services
{
    public interface ITranslationService
    {
        Task<TranslationResponse> TranslateAsync(TranslationRequest request);
        Task<TranslationResponse> ForceTranslateAsync(TranslationRequest request);
    }

    public class TranslationService : ITranslationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _endpointUrl;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _configuration;
        private readonly DistributedCacheEntryOptions _cacheOptions;

        public TranslationService(
            HttpClient httpClient,
            IConfiguration configuration,
            IDistributedCache cache)
        {
            _httpClient = httpClient;
            _endpointUrl = configuration["TranslationService:Endpoint"]
                ?? throw new ArgumentNullException("TranslationService:Endpoint configuration is missing.");
            _cache = cache;
            _configuration = configuration;

            var cacheExpirationMinutes = _configuration.GetValue<int>("Cache:ExpirationMinutes", 30);
            _cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheExpirationMinutes)
            };
        }

        public async Task<TranslationResponse> TranslateAsync(TranslationRequest request)
        {
            var cacheKey = GenerateCacheKey(request);

            var cachedResponse = await _cache.GetStringAsync(cacheKey);
            if (cachedResponse != null)
            {
                return JsonSerializer.Deserialize<TranslationResponse>(cachedResponse)!;
            }

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_endpointUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();

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

            var translationResponse = new TranslationResponse
            {
                Language = request.Language,
                Messages = messages
            };

            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(translationResponse),
                _cacheOptions);

            return translationResponse;
        }

        public async Task<TranslationResponse> ForceTranslateAsync(TranslationRequest request)
        {
            var cacheKey = GenerateCacheKey(request);

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_endpointUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();

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

            var translationResponse = new TranslationResponse
            {
                Language = request.Language,
                Messages = messages
            };

            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(translationResponse),
                _cacheOptions);

            return translationResponse;
        }

        private string GenerateCacheKey(TranslationRequest request)
        {
            var messagesKey = string.Join("|", request.Messages.Select(m => m.Message));
            return $"translation_{request.SourceLanguage}_{request.Language}_{messagesKey}";
        }
    }
}