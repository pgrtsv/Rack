using System.Windows;

namespace Rack.LocalizationTool
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = new MainWindowViewModel(this); 
            InitializeComponent();
        }
    }
}
