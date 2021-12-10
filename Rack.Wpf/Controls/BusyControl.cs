using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Rack.Wpf.Controls
{
    public class BusyControl : Grid
    {
        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(
            "IsBusy", typeof(bool), typeof(BusyControl), new PropertyMetadata(IsBusyChangedCallback));

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
            "Status", typeof(string), typeof(BusyControl));

        public BusyControl()
        {
            var textBlock = new TextBlock();
            Focusable = false;
            BindingOperations.SetBinding(textBlock, TextBlock.TextProperty,
                new Binding("Status")
                    {RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(BusyControl), 1)});

            ContentWhenBusy = new Grid
            {
                Background = new SolidColorBrush(new Color{ A = 100, R = 255, G = 255, B =255})
            };
            ContentWhenBusy.Children.Add(new StackPanel
            {
                Children =
                {
                    new ProgressBar
                    {
                        Value = 0,
                        Style = FindResource("MaterialDesignCircularProgressBar") as Style,
                        IsIndeterminate = true
                    },
                    textBlock
                },
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            });
        }

        public string Status
        {
            get => (string) GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        public bool IsBusy
        {
            get => (bool) GetValue(IsBusyProperty);
            set => SetValue(IsBusyProperty, value);
        }

        public Grid ContentWhenBusy { get; }

        private static void IsBusyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is true)
                ((BusyControl) d).SwitchToBusyMode();
            else
                ((BusyControl) d).SwitchToFreeMode();
        }

        private void SwitchToBusyMode()
        {
            Children.Add(ContentWhenBusy);
            IsEnabled = false;
        }

        private void SwitchToFreeMode()
        {
            Children.Remove(ContentWhenBusy);
            IsEnabled = true;
        }
    }
}