using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using CozyPlayer.Resources.Styles;

namespace CozyPlayer.Services
{
    public class ThemeService
    {
        public const string PreferencesKey = "AppThemeKey";
        public static readonly string DefaultTheme = "CozyGreen";

        public static ThemeService Instance { get; } = new ThemeService();

        public event EventHandler<string> ThemeChanged;

        private string _currentTheme = DefaultTheme;
        public string CurrentTheme => _currentTheme;

        private ThemeService() { }

        public string[] GetAvailableThemes() => new[] { "CozyGreen", "CozyPurple", "CozyBlue" };

        public void ApplySavedTheme()
        {
            var saved = Preferences.Get(PreferencesKey, DefaultTheme);
            ApplyTheme(saved);
        }

        public void ApplyTheme(string themeName)
        {
            if (string.IsNullOrEmpty(themeName)) themeName = DefaultTheme;
            if (_currentTheme == themeName) return;

            _currentTheme = themeName;

            // Удаляем все наши theme ResourceDictionaries, затем добавляем нужный
            var appResources = Application.Current?.Resources;
            if (appResources == null) return;

            // Удаляем предыдущие словари тем (по их типам)
            var toRemove = appResources.MergedDictionaries
                .Where(d => d.GetType().Namespace == typeof(CozyGreen).Namespace)
                .ToList();

            foreach (var d in toRemove) appResources.MergedDictionaries.Remove(d);

            // Добавляем выбранную тему
            ResourceDictionary themeDict = themeName switch
            {
                "CozyPurple" => new CozyPurple(),
                "CozyBlue" => new CozyBlue(),
                _ => new CozyGreen()
            };

            appResources.MergedDictionaries.Add(themeDict);

            // Сохраняем выбор
            Preferences.Set(PreferencesKey, themeName);

            ThemeChanged?.Invoke(this, themeName);
        }
    }
}
