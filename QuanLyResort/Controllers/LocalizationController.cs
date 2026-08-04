using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyResort.Services;

namespace QuanLyResort.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocalizationController : ControllerBase
{
    private readonly ILocalizationService _localizationService;

    public LocalizationController(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    [HttpGet("strings")]
    public IActionResult GetStrings([FromQuery] string? lang = null)
    {
        var language = lang ?? _localizationService.GetCurrentLanguage();
        var strings = new Dictionary<string, string>();

        // Get all translation keys (simplified - in production, load from resource files)
        var keys = new[]
        {
            "common.save", "common.cancel", "common.delete", "common.edit",
            "auth.login", "auth.logout", "auth.register",
            "booking.title", "booking.check_in", "booking.check_out",
            "payment.title", "payment.method", "payment.amount"
        };

        foreach (var key in keys)
        {
            strings[key] = _localizationService.GetString(key, language);
        }

        return Ok(new { language, strings });
    }

    [HttpPost("set-language")]
    [Authorize]
    public IActionResult SetLanguage([FromBody] SetLanguageRequest request)
    {
        if (!_localizationService.IsLanguageSupported(request.Language))
        {
            return BadRequest(new { message = "Language not supported" });
        }

        _localizationService.SetLanguage(request.Language);
        return Ok(new { message = "Language changed successfully", language = request.Language });
    }

    [HttpGet("current")]
    public IActionResult GetCurrentLanguage()
    {
        return Ok(new { language = _localizationService.GetCurrentLanguage() });
    }

    [HttpGet("supported")]
    public IActionResult GetSupportedLanguages()
    {
        return Ok(new { languages = _localizationService.GetSupportedLanguages() });
    }
}

public class SetLanguageRequest
{
    public string Language { get; set; } = "vi";
}

