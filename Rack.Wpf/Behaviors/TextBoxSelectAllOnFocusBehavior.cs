using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace Rack.Wpf.Behaviors
{
    /// <summary>
    /// Выделяет содержимое текстбокса при получении им фокуса.
    /// </summary>
    public sealed class TextBoxSelectAllOnFocusBehavior: Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            AssociatedObject.GotFocus += OnTextBoxGotFocus;
        }

        private void OnTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox) sender;
            textBox.SelectAll();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.GotFocus -= OnTextBoxGotFocus;
        }
    }
}