using System;
using System.Globalization;

namespace Rack.GeoTools
{
    public static class ScaleConvert
    {
        public static string Convert(double scale)
        {
            if (!double.IsFinite(scale))
                return "-";
            if (scale <= 0)
                return "-";
            var reversedValue = (int)Math.Round(1.0 / scale);
            return $"1:{reversedValue:##,#}";
        }

        public static double ConvertFrom(string text)
        {
            if (int.TryParse(text.Substring(text.IndexOf(':') + 1), 
                NumberStyles.AllowThousands,
                CultureInfo.CurrentCulture, 
                out var reversedValue))
                return 1.0 / reversedValue;
            return double.NaN;
        }
    }
}