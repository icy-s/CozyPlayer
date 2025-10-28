using Microsoft.Maui.Controls;
using CozyPlayer.Services;
using System.Diagnostics;

namespace CozyPlayer.Views;

public partial class Settings : ContentPage
{
    public Settings() => InitializeComponent();

    private void OnThemeChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!e.Value) return;
        if (sender is RadioButton rb && rb.Value != null)
        {
            var theme = rb.Value.ToString();
            Debug.WriteLine($"[Settings] Theme selected: {theme}");
            ThemeService.Instance.ApplyTheme(theme);
        }
    }

    private void OnLanguageClicked(object sender, EventArgs e)
    {
        if (sender is Button b && b.CommandParameter is string lang)
        {
            LocalizationResourceManager.Instance.SetCulture(new System.Globalization.CultureInfo(lang));
        }
    }
}
