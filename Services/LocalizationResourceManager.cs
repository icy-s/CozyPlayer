using System;
using System.ComponentModel;
using System.Globalization;
using CozyPlayer.Resources.Localization;
using Microsoft.Maui.Storage;

namespace CozyPlayer.Services
{
    public class LocalizationResourceManager : INotifyPropertyChanged
    {
        public static LocalizationResourceManager Instance { get; } = new LocalizationResourceManager();

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler LanguageChanged;

        private LocalizationResourceManager() { }

        public string this[string text]
        {
            get
            {
                if (string.IsNullOrEmpty(text)) return string.Empty;
                var val = AppResources.ResourceManager.GetString(text, CultureInfo.CurrentUICulture);
                return val ?? text;
            }
        }

        public void SetCulture(CultureInfo ci)
        {
            if (ci == null) return;

            CultureInfo.DefaultThreadCurrentCulture = ci;
            CultureInfo.DefaultThreadCurrentUICulture = ci;

            // оповещение для подписчиков (Markupextension использует его)
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            LanguageChanged?.Invoke(this, EventArgs.Empty);

            // сохраняем выбор
            Preferences.Set("AppLanguage", ci.Name);
        }

        public CultureInfo GetCurrentCulture() => CultureInfo.CurrentUICulture;
    }
}
