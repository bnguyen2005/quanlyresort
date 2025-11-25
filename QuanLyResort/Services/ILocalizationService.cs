namespace QuanLyResort.Services;

public interface ILocalizationService
{
    string GetString(string key, string? language = null);
    string GetString(string key, object? parameters, string? language = null);
    string GetCurrentLanguage();
    void SetLanguage(string language);
    bool IsLanguageSupported(string language);
    List<string> GetSupportedLanguages();
}

