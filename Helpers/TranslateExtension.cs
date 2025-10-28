using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using CozyPlayer.Services;

namespace CozyPlayer.Helpers
{
    [ContentProperty(nameof(Text))]
    public class TranslateExtension : IMarkupExtension
    {
        public string Text { get; set; }

        // Подписка и прямое обновление целевого свойства UI при смене языка
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) return null;

            var provide = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
            var targetObject = provide?.TargetObject as BindableObject;
            var targetProperty = provide?.TargetProperty as BindableProperty;

            if (targetObject == null || targetProperty == null)
            {
                // для шаблонов/других сценариев — просто возвращаем текущую строку
                return LocalizationResourceManager.Instance[Text] ?? Text;
            }

            // установить начальное значение
            targetObject.SetValue(targetProperty, LocalizationResourceManager.Instance[Text] ?? Text);

            // подписываемся на изменение языка
            LocalizationResourceManager.Instance.LanguageChanged += (s, e) =>
            {
                try
                {
                    targetObject.SetValue(targetProperty, LocalizationResourceManager.Instance[Text] ?? Text);
                }
                catch { /* ignore if element disposed */ }
            };

            return targetObject.GetValue(targetProperty);
        }
    }
}
