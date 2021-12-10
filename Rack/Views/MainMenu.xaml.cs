using Rack.ViewModels;
using ReactiveUI;

namespace Rack.Views
{
    public partial class MainMenu : ReactiveUserControl<MainMenuViewModel>
    {
        public MainMenu()
        {
            InitializeComponent();

            this.WhenActivated(cleanUp =>
            {
                Menu.ItemsSource = ViewModel.ApplicationTabs;
            });
        }
    }
}