using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Rack.Wpf.Converters
{
    public class UtcToLocalDateTimeConverter : MarkupExtension, IValueConverter
    {
        public static readonly UtcToLocalDateTimeConverter Instance = new UtcToLocalDateTimeConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DateTime dateTime)) return null;
            return dateTime.ToLocalTime();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DateTime dateTime)) return null;
            return dateTime.ToUniversalTime();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }
    }
}