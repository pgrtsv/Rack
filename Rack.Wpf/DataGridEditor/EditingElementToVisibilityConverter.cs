using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Rack.Wpf.DataGridEditor
{
    public class EditingElementToVisibilityConverter : IMultiValueConverter
    {
        /// <inheritdoc />
        public object Convert(object[] values, Type targetType, object parameter,
            CultureInfo culture)
        {
            if (values.Length != 2) throw new ArgumentException();
            var editingObject = values[0];
            var bindingObject = values[1];
            if (editingObject == null || bindingObject == null) return Visibility.Collapsed;
            return editingObject == bindingObject ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <inheritdoc />
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            CultureInfo culture) => throw new NotImplementedException();
    }
}