using CozyPlayer.Resources.Styles;

namespace CozyPlayer.Services
{
    public class ThemeService
    {
        public static ThemeService Instance { get; } = new ThemeService();
        const string KEY = "AppTheme";
        private string _currentTheme = "CozyGreen";
        public string CurrentTheme => _currentTheme;

        public event EventHandler<string> ThemeChanged;

        private ThemeService() { }

        public void ApplySavedTheme()
        {
            var t = Preferences.Get(KEY, "CozyGreen");
            ApplyTheme(t);
        }

        public void ApplyTheme(string themeName)
        {
            if (string.IsNullOrEmpty(themeName)) themeName = "CozyGreen";
            if (_currentTheme == themeName) return;

            _currentTheme = themeName;

            // удалить старые словари из того же namespace
            var themeNamespace = typeof(CozyGreen).Namespace;
            var toRemove = Application.Current.Resources.MergedDictionaries
                .Where(d => d.GetType().Namespace == themeNamespace)
                .ToList();
            foreach (var d in toRemove)
                Application.Current.Resources.MergedDictionaries.Remove(d);

            // добавить новый словарь
            ResourceDictionary dict = themeName switch
            {
                "CozyBlue" => new CozyBlue(),
                "CozyPurple" => new CozyPurple(),
                _ => new CozyGreen(),
            };

            Application.Current.Resources.MergedDictionaries.Add(dict);
            Preferences.Set(KEY, themeName);

            ThemeChanged?.Invoke(this, themeName);
        }
    }
}