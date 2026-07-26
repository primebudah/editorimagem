using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AIImageChat.Converters
{
    /// <summary>
    /// Conversor de zoom para ScaleTransform
    /// </summary>
    public class ZoomToTransformConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double zoom)
            {
                return new ScaleTransform(zoom, zoom);
            }
            return new ScaleTransform(1.0, 1.0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
