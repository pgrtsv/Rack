using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Xaml.Behaviors;

namespace Rack.Wpf.Behaviors
{
    public class UpdateOnExecuteCommandBehavior : Behavior<Button>
    {
        public static readonly DependencyProperty EditingRootProperty = DependencyProperty.Register(
            "EditingRoot", typeof(UIElement), typeof(UpdateOnExecuteCommandBehavior));

        public UIElement EditingRoot
        {
            get => (UIElement) GetValue(EditingRootProperty);
            set => SetValue(EditingRootProperty, value);
        }

        protected override void OnAttached()
        {
            AssociatedObject.Click += AssociatedObjectOnClick;
        }

        private void AssociatedObjectOnClick(object sender, RoutedEventArgs e)
        {
            foreach (var binding in BindingOperations.GetSourceUpdatingBindings(EditingRoot))
                binding.UpdateSource();
            //foreach (var child in EditingRoot.FindChildren<UIElement>())
            //{
            //    foreach (var binding in BindingOperations.GetSourceUpdatingBindings(child))
            //        binding.UpdateSource();
            //}
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Click -= AssociatedObjectOnClick;
        }
    }
}