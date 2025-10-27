using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace CozyPlayer.Helpers
{
    public class BoolToPlayPauseTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b) return "❚❚ Pause";
            return "▶ Play";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
