using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace Rack.Wpf.DataGridEditor
{
    public class DataGridAdditionalHeader : Grid
    {
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header", typeof(string), typeof(DataGridAdditionalHeader),
            new PropertyMetadata(default(string), HeaderChanged));

        private static void HeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var additionalHeader = (DataGridAdditionalHeader) d;
            additionalHeader._headerTextBlock.Text = e.NewValue as string;
        }

        public string Header
        {
            get => (string) GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty RightPanelContentProperty = DependencyProperty.Register(
            "RightPanelContent", typeof(UIElement), typeof(DataGridAdditionalHeader),
            new PropertyMetadata(default(UIElement), RightPanelContentChanged));

        private static void RightPanelContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var additionalHeader = (DataGridAdditionalHeader) d;
            var newContent = e.NewValue as UIElement;
            additionalHeader._rightBorder.Child = newContent;

            if (additionalHeader._rightBorder != null)
                additionalHeader._rightBorder.BorderThickness = newContent == null
                    ? new Thickness(0, 0 ,0, 1)
                    : new Thickness(1, 0, 0, 1);
        }

        public UIElement RightPanelContent
        {
            get => (UIElement) GetValue(RightPanelContentProperty);
            set => SetValue(RightPanelContentProperty, value);
        }

        private readonly Border _rightBorder;
        private readonly TextBlock _headerTextBlock;

        public DataGridAdditionalHeader()
        {
            Height = 53;
            ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)});
            ColumnDefinitions.Add(new ColumnDefinition {Width = GridLength.Auto});

            _headerTextBlock = new TextBlock
            {
                Style = (Style) Application.Current.TryFindResource("MaterialDesignHeadline6TextBlock"),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5)
            };

            var mainBorder = new Border
            {
                BorderBrush = Application.Current.TryFindResource("MaterialDesignDivider") as Brush,
                Background = Application.Current.TryFindResource("MaterialDesignPaper") as Brush,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Child = _headerTextBlock
            };

            _rightBorder = new Border
            {
                BorderBrush = Application.Current.TryFindResource("MaterialDesignDivider") as Brush,
                Background = Application.Current.TryFindResource("MaterialDesignPaper") as Brush,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Child = RightPanelContent
            };
            var buttonStyle = new Style(typeof(Button), (Style)Application.Current.TryFindResource(ToolBar.ButtonStyleKey));
            Resources.Add(typeof(Button), buttonStyle);
            var packIconStyle = new Style(typeof(PackIcon));
            packIconStyle.Setters.Add(new Setter(PackIcon.WidthProperty, 20.0));
            packIconStyle.Setters.Add(new Setter(PackIcon.HeightProperty, 20.0));
            Resources.Add(typeof(PackIcon), packIconStyle);

            SetColumn(_rightBorder, 1);

            Children.Add(mainBorder);
            Children.Add(_rightBorder);
        }
    }
}