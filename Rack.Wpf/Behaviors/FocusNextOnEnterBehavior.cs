using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Rack.Wpf.Behaviors
{
    /// <summary>
    /// Смещает фокус на следующий элемент при нажатии кнопки Enter.
    /// </summary>
    public class FocusNextOnEnterBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            AssociatedObject.PreviewKeyDown += OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            e.Handled = true;
            ((FrameworkElement) e.OriginalSource).MoveFocus(
                new TraversalRequest(FocusNavigationDirection.Next));
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewKeyDown -= OnKeyDown;
        }
    }
}