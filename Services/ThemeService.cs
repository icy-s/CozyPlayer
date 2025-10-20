namespace CozyPlayer.Services;

public static class ThemeService
{
    const string ThemeKey = "AppTheme";

    public static void ApplyTheme(AppTheme theme)
    {
        Application.Current.UserAppTheme = theme;
        Preferences.Set(ThemeKey, theme.ToString());
    }

    public static void LoadTheme()
    {
        var saved = Preferences.Get(ThemeKey, AppTheme.Light.ToString());
        Application.Current.UserAppTheme = Enum.Parse<AppTheme>(saved);
    }
}