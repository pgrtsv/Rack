using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using CustomColor = Rack.GeoTools.Color;
using WpfColor = System.Windows.Media.Color;

namespace Rack.GeoTools.Wpf.Converters
{
    public class ColorToBrushConverter : MarkupExtension, IValueConverter
    {
        public static readonly ColorToBrushConverter Instance = new ColorToBrushConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = (CustomColor) value;
            return new SolidColorBrush(WpfColor.FromArgb(color.A, color.R, color.G, color.B));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }
    }
}