using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace Rack.Wpf.Converters
{
    /// <summary>
    /// Конвертер, преобразующий массив объектов в строку, и наоборот.
    /// </summary>
    public class ArrayToStringConverter : MarkupExtension, IValueConverter
    {
        private static ArrayToStringConverter Instance { get; } = new ArrayToStringConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            var array = (Array) value;
            return string.Join("; ", array.Cast<object>());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = (string) value;
            if (string.IsNullOrEmpty(text)) return null;
            try
            {
                var array = text
                    .Split(';')
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x =>
                    {
                        var normalizedText = new string(x.Where(y => !char.IsSeparator(y)).ToArray());
                        return System.Convert.ChangeType(normalizedText, targetType.GetElementType(),
                            CultureInfo.CurrentCulture);
                    })
                    .ToArray();
                var ret = Array.CreateInstance(targetType.GetElementType(), array.Length);
                Array.Copy(array, ret, array.Length);
                return ret;
            }
            catch
            {
                throw new FormatException("Введите только десятичные дроби, разделённые знаком \";\".");
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }
    }
}