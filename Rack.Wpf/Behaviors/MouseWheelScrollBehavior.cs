using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Rack.Wpf.Behaviors
{
    public class MouseWheelScrollBehavior : Behavior<UIElement>
    {
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
            "IsEnabled", typeof(bool), typeof(MouseWheelScrollBehavior), new PropertyMetadata(false));

        public bool IsEnabled
        {
            get => (bool) GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }

        protected override void OnAttached()
        {
            AssociatedObject.PreviewMouseWheel += AssociatedObjectOnPreviewMouseWheel;
        }

        private void AssociatedObjectOnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (IsEnabled) return;
            e.Handled = true;
            var eClone = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent
            };
            AssociatedObject.RaiseEvent(eClone);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseWheel -= AssociatedObjectOnPreviewMouseWheel;
        }
    }
}