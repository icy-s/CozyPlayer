using CozyPlayer.Services;

namespace CozyPlayer.Views;

public partial class Settings : ContentPage
{
    public Settings()
    {
        InitializeComponent();
        UpdateChecks();
    }

    private void UpdateChecks()
    {
        var current = ThemeService.Instance.CurrentTheme;
        GreenCheck.IsVisible = current == "CozyGreen";
        BlueCheck.IsVisible = current == "CozyBlue";
        PurpleCheck.IsVisible = current == "CozyPurple";
    }
    private void OnLanguageClicked(object sender, EventArgs e)
    {
        if (sender is Button b && b.CommandParameter is string lang)
        {
            LocalizationResourceManager.Instance.SetCulture(new System.Globalization.CultureInfo(lang));
        }
    }

    private void OnCozyGreenClicked(object sender, EventArgs e)
    {
        ThemeService.Instance.ApplyTheme("CozyGreen");
        UpdateChecks();
    }

    private void OnCozyBlueClicked(object sender, EventArgs e)
    {
        ThemeService.Instance.ApplyTheme("CozyBlue");
        UpdateChecks();
    }

    private void OnCozyPurpleClicked(object sender, EventArgs e)
    {
        ThemeService.Instance.ApplyTheme("CozyPurple");
        UpdateChecks();
    }
}
