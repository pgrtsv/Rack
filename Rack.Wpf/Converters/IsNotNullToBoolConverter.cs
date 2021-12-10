using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Rack.Wpf.Converters
{
    public class IsNotNullToBoolConverter : MarkupExtension, IValueConverter
    {
        private static IsNotNullToBoolConverter _converter;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _converter ?? (_converter = new IsNotNullToBoolConverter());
        }
    }
}