using CozyPlayer.Resources.Styles;
using Microsoft.Maui.Storage;
using System.Diagnostics;

namespace CozyPlayer.Services
{
    public class ThemeService
    {
        public static ThemeService Instance { get; } = new ThemeService();
        private const string KEY = "AppTheme";
        private string _currentTheme = "CozyGreen";
        public string CurrentTheme => _currentTheme;

        public event EventHandler<string> ThemeChanged;

        private ThemeService() { }

        public void ApplySavedTheme()
        {
            var theme = Preferences.Get(KEY, "CozyGreen");
            ApplyTheme(theme);
        }

        public void ApplyTheme(string themeName)
        {
            if (string.IsNullOrEmpty(themeName))
                themeName = "CozyGreen";

            if (_currentTheme == themeName)
                return;

            _currentTheme = themeName;

            Debug.WriteLine($"[ThemeService] Applying theme: {themeName}");

            var themeNamespace = typeof(CozyGreen).Namespace;
            var toRemove = Application.Current.Resources.MergedDictionaries
                .Where(d => d.GetType().Namespace == themeNamespace)
                .ToList();

            foreach (var d in toRemove)
                Application.Current.Resources.MergedDictionaries.Remove(d);

            ResourceDictionary dict = themeName switch
            {
                "CozyBlue" => new CozyBlue(),
                "CozyPurple" => new CozyPurple(),
                _ => new CozyGreen(),
            };

            Debug.WriteLine($"[ThemeService] Loaded dict type: {dict.GetType()}");
            Debug.WriteLine($"[ThemeService] Contains BackgroundImage: {dict.ContainsKey("BackgroundImage")}");

            Application.Current.Resources.MergedDictionaries.Add(dict);
            Preferences.Set(KEY, themeName);
            ThemeChanged?.Invoke(this, themeName);
        }

    }
}