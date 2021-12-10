using System.Windows;

namespace Rack.Wpf.DataGridEditor
{
    public class DataGridEditorAssist
    {
        public struct Empty
        {
        }

        public static readonly Empty EmptyValue = new Empty();

        public static readonly DependencyProperty DataTemplateProperty = DependencyProperty.RegisterAttached(
            "DataTemplate", typeof(DataTemplate), typeof(DataGridEditorAssist), new PropertyMetadata(default(DataTemplate)));

        public static void SetDataTemplate(DependencyObject element, DataTemplate value) => 
            element.SetValue(DataTemplateProperty, value);

        public static DataTemplate GetDataTemplate(DependencyObject element) => 
            (DataTemplate) element.GetValue(DataTemplateProperty);

        public static readonly DependencyProperty EditingObjectProperty = DependencyProperty.Register(
            "EditingObject", typeof(object), typeof(DataGridEditorAssist));

        public static void SetEditingObject(DependencyObject element, object value)
        {
            if (value is Empty) return;
            element.SetValue(EditingObjectProperty, value);
        }

        public static object GetEditingElement(DependencyObject element, object value) =>
            element.GetValue(EditingObjectProperty);
    }
}