using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Rack.LocalizationTool.Infrastructure.Behaviors
{
    /// <summary>
    /// Поведение, для открытия файла, ассоциированного с элементом интерфейсом, по двойному клику.
    /// </summary>
    public class OpenFileBehavior: Behavior<UIElement>
    {
        /// <summary>
        /// Путь к файлу, ассоциированного с элементом интерфейсом.
        /// </summary>
        public string FilePath
        {
            get => (string)GetValue(FilePathProperty);
            set => SetValue(FilePathProperty, value);
        }

        public static readonly DependencyProperty FilePathProperty =
            DependencyProperty.Register("FilePath", typeof(string), typeof(Behavior), new PropertyMetadata(""));

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeftButtonDown += MouseDownEventHandler;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseLeftButtonDown -= MouseDownEventHandler;
        }

        /// <summary>
        /// Открывет файл при двойном клике, если <see cref="FilePath"/> - не пустой.
        /// </summary>
        private void MouseDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount != 2 || string.IsNullOrEmpty(FilePath)) return;
            Process.Start(new ProcessStartInfo(FilePath)
            {
                UseShellExecute = true
            });
        }
    }
}