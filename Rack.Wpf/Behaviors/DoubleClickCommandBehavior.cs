using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Rack.Wpf.Behaviors
{
    public class DoubleClickCommandBehavior : Behavior<UIElement>
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command", typeof(ICommand), typeof(DoubleClickCommandBehavior));

        public ICommand Command
        {
            get => (ICommand) GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttached()
        {
            AssociatedObject.PreviewMouseDown += AssociatedObjectOnPreviewMouseDown;
        }

        private void AssociatedObjectOnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2) return;
            if (Command != null && Command.CanExecute(null))
                Command.Execute(null);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseDown -= AssociatedObjectOnPreviewMouseDown;
        }
    }
}