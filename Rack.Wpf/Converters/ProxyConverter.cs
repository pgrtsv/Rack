using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Rack.Wpf.Converters
{
    /// <summary>
    /// Конвертер, использующийся в привязках, где не нужно обновлять источник привязки, если новое значение равно старому.
    /// В первую очередь полезно для привязок к double и float, где Binding.UpdateSourceTrigger ==
    /// UpdateSourceTrigger.PropertyChanged и Binding.Mode == BindingMode.TwoWay.
    /// </summary>
    public class ProxyConverter : MarkupExtension, IValueConverter
    {
        public static ProxyConverter Instance => new ProxyConverter();
        private object _lastValue;
        private string _userString;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value.Equals(_lastValue) && _userString != null)
                return _userString;
            _lastValue = value;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            _userString = value?.ToString();
            return _userString;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }
    }
}