namespace LLMTranslateService.Api.Models
{
    public class TranslationMessage
    {
        public string Message { get; set; }
    }

    public class TranslationRequest
    {
        public string SourceLanguage { get; set; }
        public string Language { get; set; }
        public List<TranslationMessage> Messages { get; set; }
    }

    public class TranslationResponse
    {
        public string Language { get; set; }
        public List<TranslationMessage> Messages { get; set; }
    }
}
