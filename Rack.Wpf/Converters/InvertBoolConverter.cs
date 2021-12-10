using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Rack.Wpf.Converters
{
    public class InvertBoolConverter: MarkupExtension, IValueConverter
    {
        public static readonly InvertBoolConverter Instance = new InvertBoolConverter();
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}