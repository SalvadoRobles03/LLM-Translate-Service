using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LLMTranslateService.Api.Models
{
    public class TranslationMessage
    {
        [JsonPropertyName("message")]
        public required string Message { get; set; }
    }

    public class TranslationRequest
    {
        [JsonPropertyName("SourceLanguage")]
        public required string SourceLanguage { get; set; }

        [JsonPropertyName("language")]
        public required string Language { get; set; }

        [JsonPropertyName("Messages")]
        public required List<TranslationMessage> Messages { get; set; }
    }

    public class TranslationResponse
    {
        [JsonPropertyName("language")]
        public required string Language { get; set; }

        [JsonPropertyName("Messages")]
        public required List<TranslationMessage> Messages { get; set; }
    }
}
