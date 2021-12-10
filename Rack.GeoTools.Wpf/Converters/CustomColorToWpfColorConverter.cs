using System;
using ReactiveUI;
using WpfColor = System.Windows.Media.Color;
using CustomColor = Rack.GeoTools.Color;

namespace Rack.GeoTools.Wpf.Converters
{
    public sealed class CustomColorToWpfColorConverter : IBindingTypeConverter
    {
        /// <inheritdoc />
        public int GetAffinityForObjects(Type fromType, Type toType) =>
            fromType == typeof(CustomColor) && toType == typeof(WpfColor) ||
            fromType == typeof(WpfColor) && toType == typeof(CustomColor)
                ? 10
                : 0;

        /// <inheritdoc />
        public bool TryConvert(object @from, Type toType, object conversionHint, out object result)
        {
            if (@from == null)
            {
                result = default;
                return false;
            }

            if (toType == typeof(WpfColor))
            {
                var color = (CustomColor) @from;
                result = WpfColor.FromArgb(
                    color.A,
                    color.R,
                    color.G,
                    color.B);
                return true;
            }

            if (toType == typeof(CustomColor))
            {
                var color = (WpfColor) @from;
                result = new CustomColor(
                    color.R,
                    color.G,
                    color.B,
                    color.A);
                return true;
            }

            result = default;
            return false;
        }
    }
}