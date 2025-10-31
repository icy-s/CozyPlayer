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

            // Remove old theme dictionaries (from our namespace)
            var themeNamespace = typeof(CozyGreen).Namespace;
            var toRemove = Application.Current.Resources.MergedDictionaries
                .Where(d => d.GetType().Namespace == themeNamespace)
                .ToList();

            foreach (var d in toRemove)
                Application.Current.Resources.MergedDictionaries.Remove(d);

            // Create new ResourceDictionary from XAML type
            ResourceDictionary dict = themeName switch
            {
                "CozyBlue" => new CozyBlue(),
                "CozyPurple" => new CozyPurple(),
                _ => new CozyGreen(),
            };

            Application.Current.Resources.MergedDictionaries.Add(dict);

            // Save preference
            Preferences.Set(KEY, themeName);

            // Notify subscribers
            ThemeChanged?.Invoke(this, themeName);
        }
    }
}