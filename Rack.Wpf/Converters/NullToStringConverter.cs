using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Rack.Wpf.Converters
{
    /// <summary>
    /// Если объект - null, то возвращает параметр конвертера.
    /// </summary>
    public class NullToStringConverter : MarkupExtension, IValueConverter
    {
        public static NullToStringConverter Instance { get; } = new NullToStringConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ?? parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(parameter) ? null : value;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }
    }
}