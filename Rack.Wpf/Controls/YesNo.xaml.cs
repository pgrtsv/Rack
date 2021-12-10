using System.Reactive.Disposables;
using ReactiveUI;

namespace Rack.Wpf.Controls
{
    public partial class YesNo : ReactiveUserControl<YesNoViewModel>
    {
        public YesNo()
        {
            InitializeComponent();

            this.WhenActivated(cleanUp =>
            {
                this.BindCommand(
                        ViewModel,
                        x => x.YesCommand,
                        x => x.YesButton)
                    .DisposeWith(cleanUp);
                this.BindCommand(
                        ViewModel,
                        x => x.NoCommand,
                        x => x.NoButton)
                    .DisposeWith(cleanUp);
                this.OneWayBind(
                    ViewModel,
                    x => x.Content,
                    x => x.ContentTextBlock.Text)
                    .DisposeWith(cleanUp);
            });
        }
    }
}
