using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Rack.GeoTools.Wpf.Converters
{
    public class ScaleConverter : MarkupExtension, IValueConverter
    {
        private static readonly ScaleConverter Instance = new ScaleConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double doubleValue)) 
                throw new ArgumentException(nameof(value));
            return ScaleConvert.Convert(doubleValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string stringValue)) throw new ArgumentException(nameof(value));
            return ScaleConvert.ConvertFrom(stringValue);
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => Instance;
    }
}