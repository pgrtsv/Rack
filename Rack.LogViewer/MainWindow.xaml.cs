using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ReactiveUI;

namespace Rack.LogViewer
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            DataContext = ViewModel = new MainWindowViewModel();
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel, x => x.FilePath, x => x.Title, x => string.IsNullOrEmpty(x) ? "LogViewer" : "LogViewer - " + x);
                this.Bind(ViewModel, x => x.FilePath, x => x.FilePathTextBox.Text);
                this.OneWayBind(ViewModel, x => x.Logs, x => x.LogsListView.ItemsSource);
                this.OneWayBind(ViewModel, x => x.IsBusy, x => x.ProgressBar.Visibility,
                    x => x ? Visibility.Visible : Visibility.Collapsed);
                this.OneWayBind(ViewModel, x => x.Levels, x => x.LevelsItemsControl.ItemsSource);
                SortComboBox.ItemsSource = ViewModel.SortProperties;
                this.Bind(ViewModel, x => x.SelectedSortProperty, x => x.SortComboBox.SelectedItem);
                this.Bind(ViewModel, x => x.IsSortAscending, x => x.SortToggleButton.IsChecked);
            });
        }
    }
}
