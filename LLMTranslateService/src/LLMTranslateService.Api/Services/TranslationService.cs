using LLMTranslateService.Api.Models;
using System.Collections.Generic;
using System.Linq;

namespace LLMTranslateService.Api.Services
{
    public interface ITranslationService
    {
        TranslationResponse Translate(TranslationRequest request);
    }

    public class TranslationService : ITranslationService
    {
        public TranslationResponse Translate(TranslationRequest request)
        {
            var translatedMessages = request.Messages.Select(m => new TranslationMessage
            {
                Message = $"[Translated to {request.Language}] {m.Message}"
            }).ToList();

            return new TranslationResponse
            {
                Language = request.Language,
                Messages = translatedMessages
            };
        }
    }
}
