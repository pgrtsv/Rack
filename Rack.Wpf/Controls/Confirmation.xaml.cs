using System.Windows.Controls;

namespace Rack.Wpf.Controls
{
    public partial class Confirmation : UserControl
    {
        public Confirmation()
        {
            DataContext = new ConfirmationViewModel();
            InitializeComponent();
        }
    }
}