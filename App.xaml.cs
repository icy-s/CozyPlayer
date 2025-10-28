using CozyPlayer.Services;
using System.Globalization;

namespace CozyPlayer;

public partial class App : Application
{
    public static DatabaseService Database { get; private set; }

    public App()
    {
        InitializeComponent();

        // apply theme
        CozyPlayer.Services.ThemeService.Instance.ApplySavedTheme();

        // apply saved language (if there is one)
        var saved = Preferences.Get("AppLanguage", null);
        if (!string.IsNullOrEmpty(saved))
        {
            try
            {
                LocalizationResourceManager.Instance.SetCulture(new CultureInfo(saved));
            }
            catch { /* ignore */ }
        }

        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "cozyplayer.db3");
        Database = new DatabaseService(dbPath);

        MainPage = new NavigationPage(new Views.MainPage());
    }
}