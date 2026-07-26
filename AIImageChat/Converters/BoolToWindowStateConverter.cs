using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AIImageChat.Converters
{
    /// <summary>
    /// Conversor de booleano para WindowState
    /// </summary>
    public class BoolToWindowStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool isFullscreen && isFullscreen ? WindowState.Maximized : WindowState.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is WindowState state && state == WindowState.Maximized;
        }
    }
}
