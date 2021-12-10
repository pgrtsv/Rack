using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using CustomColor = Rack.GeoTools.Color;
using WpfColor = System.Windows.Media.Color;

namespace Rack.GeoTools.Wpf.Converters
{
    public class ColorConverter : MarkupExtension, IValueConverter
    {
        public static ColorConverter Instance { get; } = new ColorConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = (CustomColor) value;
            return WpfColor.FromArgb(color.A, color.R, color.G, color.B);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = (WpfColor) value;
            return new CustomColor {A = color.A, R = color.R, G = color.G, B = color.B};
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }
    }
}