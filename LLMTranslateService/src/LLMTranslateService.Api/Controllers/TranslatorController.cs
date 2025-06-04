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
        public async Task<IActionResult> Translate([FromBody] TranslationRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.SourceLanguage) ||
                string.IsNullOrWhiteSpace(request.Language) ||
                request.Messages == null || request.Messages.Count == 0)
            {
                return BadRequest("Bad Request");
            }

            var response = await _translationService.TranslateAsync(request);
            return Ok(response);
        }
        [HttpPost("force-translate")]
        public async Task<IActionResult> ForceTranslate([FromBody] TranslationRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.SourceLanguage) ||
                string.IsNullOrWhiteSpace(request.Language) ||
                request.Messages == null || request.Messages.Count == 0)
            {
                return BadRequest("Bad Request");
            }

            var response = await _translationService.ForceTranslateAsync(request);
            return Ok(response);
        }
    }
}