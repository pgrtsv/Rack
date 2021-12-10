using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace Rack.Wpf.Controls
{
    /// <summary>
    /// Контрол, добавляющий текстовое описание слева от дочерних элементов.
    /// </summary>
    [ContentProperty("InnerContent")]
    public class LabeledControl : UserControl
    {
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description",
            typeof(string), typeof(LabeledControl));

        public static readonly DependencyProperty InnerContentProperty = DependencyProperty.Register("InnerProperty",
            typeof(UIElement), typeof(LabeledControl));

        public LabeledControl()
        {
            var label = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(3, 5, 3, 5)
            };

            label.SetBinding(TextBlock.TextProperty,
                new Binding(nameof(Description))
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, GetType(), 1)
                });

            var contentControl = new ContentControl
            {
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            contentControl.SetBinding(ContentProperty,
                new Binding(nameof(InnerContent))
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, GetType(), 1)
                });

            Grid.SetColumn(label, 0);
            Grid.SetColumn(contentControl, 1);

            Content = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition {Width = GridLength.Auto, SharedSizeGroup = "DescriptionColumn"},
                    new ColumnDefinition()
                },
                Children = {label, contentControl}
            };
        }

        /// <summary>
        /// Описание контрола.
        /// </summary>
        public string Description
        {
            get => (string) GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        /// <summary>
        /// Внутреннее содержимое контрола.
        /// </summary>
        public UIElement InnerContent
        {
            get => (UIElement) GetValue(InnerContentProperty);
            set => SetValue(InnerContentProperty, value);
        }
    }
}