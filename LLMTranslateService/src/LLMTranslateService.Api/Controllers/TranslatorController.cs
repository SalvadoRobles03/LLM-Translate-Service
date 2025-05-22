using Microsoft.AspNetCore.Mvc;
using LLMTranslateService.Api.Models;
using LLMTranslateService.Api.Services;

namespace LLMTranslateService.Api.Controllers
{
    [ApiController]
    [Route("translate")]
    public class TranslationController : ControllerBase
    {
        private readonly ITranslationService _translationService;

        public TranslationController(ITranslationService translationService)
        {
            _translationService = translationService;
        }

        [HttpGet]
        public IActionResult Translate([FromBody] TranslationRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.SourceLanguage) ||
                string.IsNullOrWhiteSpace(request.Language) ||
                request.Messages == null || request.Messages.Count == 0)
            {
                return BadRequest("Bad Request");
            }

            var response = _translationService.Translate(request);
            return Ok(response);
        }
    }
}
