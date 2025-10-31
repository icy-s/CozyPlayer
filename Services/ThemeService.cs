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

        public async void ApplyTheme(string themeName)
        {
            if (string.IsNullOrEmpty(themeName))
                themeName = "CozyGreen";

            if (_currentTheme == themeName)
                return;

            _currentTheme = themeName;

            Debug.WriteLine($"[ThemeService] Applying theme: {themeName}");

            var app = Application.Current;
            if (app?.MainPage == null)
            {
                Debug.WriteLine("[ThemeService] MainPage is null, skipping animation");
                return;
            }

            // 🔹 Fade out
            await app.MainPage.FadeTo(0, 250, Easing.CubicOut);

            // Remove previous themes
            var themeNamespace = typeof(CozyGreen).Namespace;
            var toRemove = app.Resources.MergedDictionaries
                .Where(d => d.GetType().Namespace == themeNamespace)
                .ToList();

            foreach (var d in toRemove)
                app.Resources.MergedDictionaries.Remove(d);

            // Add new theme
            ResourceDictionary dict = themeName switch
            {
                "CozyBlue" => new CozyBlue(),
                "CozyPurple" => new CozyPurple(),
                _ => new CozyGreen(),
            };

            app.Resources.MergedDictionaries.Add(dict);
            Preferences.Set(KEY, themeName);

            // 🔹 Fade back in
            await app.MainPage.FadeTo(1, 250, Easing.CubicIn);

            // Notify listeners
            ThemeChanged?.Invoke(this, themeName);
        }
    }
}