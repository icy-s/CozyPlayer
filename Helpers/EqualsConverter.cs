using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace CozyPlayer.Helpers
{
    public class EqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null && parameter == null) return true;
            if (value == null || parameter == null) return false;
            return value.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // When radio becomes checked -> return parameter as SelectedTheme
            if (value is bool b && b)
                return parameter?.ToString();
            return Binding.DoNothing;
        }
    }
}