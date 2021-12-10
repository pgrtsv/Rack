using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Rack.Wpf.DataGridEditor
{
    public class EditingObjectToBoolConverter : IMultiValueConverter
    {
        private object _bindingObject;

        /// <inheritdoc />
        public object Convert(object[] values, Type targetType, object parameter,
            CultureInfo culture)
        {
            var editingObject = values[0];
            _bindingObject = values[1];
            if (editingObject == null) return false;
            return editingObject == _bindingObject;
        }

        /// <inheritdoc />
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            CultureInfo culture)
        {
            var isEditing = (bool) value;
            return new[]
                {isEditing ? _bindingObject : null, _bindingObject};
        }
    }
}