namespace CozyPlayer.Services;

public class SettingsService
{
    const string LanguageKey = "AppLanguage";

    public string CurrentLanguage
    {
        get => Preferences.Get(LanguageKey, "et");
        set => Preferences.Set(LanguageKey, value);
    }
}