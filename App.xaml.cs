using CozyPlayer.Services;

namespace CozyPlayer;

public partial class App : Application
{
    public static DatabaseService Database { get; private set; }

    public App()
    {
        InitializeComponent();

        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "cozyplayer.db3");
        Database = new DatabaseService(dbPath);

        ThemeService.LoadTheme();

        MainPage = new AppShell();
    }
}