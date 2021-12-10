using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace Rack.GeoTools.Wpf.Converters
{
    public class ThicknessToTransformConverter : MarkupExtension, IValueConverter
    {
        public static readonly ThicknessToTransformConverter Instance = new ThicknessToTransformConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var thickness = (double) value;
            return new TranslateTransform(-thickness / 2, -thickness / 2);
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