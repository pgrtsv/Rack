using System.Linq;
using System.Windows;
using System.Windows.Data;
using MahApps.Metro.Controls;

namespace Rack.Wpf.Controls
{
    public sealed class DefaultDialogWindow : MetroWindow
    {
        public DefaultDialogWindow()
        {
            SetBinding(TitleProperty,
                new Binding("Content.ViewModel.Title")
                    {RelativeSource = new RelativeSource(RelativeSourceMode.Self)});
            Style = (Style) FindResource("DialogWindow");
            MinHeight = 200;
            MinWidth = 400;
            SizeToContent = SizeToContent.WidthAndHeight;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            MaxWidth = SystemParameters.PrimaryScreenWidth - 100;
            MaxHeight = SystemParameters.PrimaryScreenHeight - 100;
        }

        protected override void OnStyleChanged(Style oldStyle, Style newStyle)
        {
            base.OnStyleChanged(oldStyle, newStyle);
            if (newStyle.Setters.OfType<Setter>().Any(x => x.Property == SizeToContentProperty))
                SizeToContent = (SizeToContent) newStyle.Setters.OfType<Setter>()
                    .First(x => x.Property == SizeToContentProperty).Value;
        }
    }
}