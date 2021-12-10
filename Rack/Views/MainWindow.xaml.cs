using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MahApps.Metro.Controls;
using Rack.ViewModels;
using Rack.Wpf.Reactive;
using ReactiveUI;

namespace Rack.Views
{
    public partial class MainWindow : MetroWindow,
        IViewFor<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();

            this.WhenActivated(cleanUp =>
            {
                var binding = new BindingHelper<MainWindow, MainWindowViewModel>(
                    this,
                    cleanUp);

                binding
                    .OneWayBind(
                        x => x.Title,
                        x => x.Title)
                    .Do(() =>
                        this.Events().PreviewKeyDown
                            .Where(x => x.Key == Key.F1)
                            .Select(_ => Unit.Default)
                            .InvokeCommand(ViewModel.ShowAppHelp)
                            .DisposeWith(cleanUp))

                    #region Window commands

                    .BindCommand(
                        x => x.ShowChangelogs,
                        x => x.ShowChangelogsButton)
                    .OneWayBind(
                        x => x.Localization["Changelogs"],
                        x => x.ShowChangelogsButton.ToolTip)
                    .BindCommand(
                        x => x.Authenticate,
                        x => x.AuthenticateButton)
                    .OneWayBind(
                        x => x.Localization["Authentication"],
                        x => x.AuthenticateButton.ToolTip)
                    .BindCommand(
                        x => x.ShowMessagesJournal,
                        x => x.ShowMessagesButton)
                    .OneWayBind(
                        x => x.Localization["MessagesJournal"],
                        x => x.ShowMessagesButton.ToolTip)
                    .BindCommand(
                        x => x.ShowAppSettings,
                        x => x.ShowAppSettingsButton)
                    .OneWayBind(
                        x => x.Localization["Settings"],
                        x => x.ShowAppSettingsButton.ToolTip)
                    .BindCommand(
                        x => x.ShowAppHelp,
                        x => x.ShowAppHelpButton)
                    .OneWayBind(
                        x => x.Localization["Help"],
                        x => x.ShowAppHelpButton.ToolTip)

                    #endregion

                    #region DialogHost

                    .Bind(
                        x => x.IsBigMessageDialogOpen,
                        x => x.DialogHost.IsOpen)
                    .BindCommand(
                        x => x.CopyCurrentBigMessageToClipboard,
                        x => x.CopyBigMessageMenuItem)
                    .OneWayBind(
                        x => x.Localization["CopyMessage"],
                        x => x.CopyBigMessageMenuItem.Header)
                    .OneWayBind(
                        x => x.CurrentBigMessage.Text,
                        x => x.BigMessageTextBlock.Text)
                    .Do(() =>
                    {
                        BigMessageCloseButton.Command =
                            MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand;
                    })

                    #endregion

                    .OneWayBind(
                        x => x.MainMenuViewModel,
                        x => x.MainMenu.ViewModel)
                    .OneWayBind(
                        x => x.SmallMessages,
                        x => x.SmallMessagesSnackbar.MessageQueue)
                    .OneWayBind(
                        x => x.IsUpdating,
                        x => x.UpdateGrid.Visibility)
                    .OneWayBind(
                        x => x.UpdateProgress,
                        x => x.UpdateProgressBar.Value)
                    .OneWayBind(
                        x => x.Localization["UpdatingApp"],
                        x => x.UpdateTextBlock.Text)
                    .OneWayBind(
                        x => x.IsMessagesJournalOpen,
                        x => x.DrawerHost.IsRightDrawerOpen)
                    .BindCommand(
                        x => x.CloseMessagesJournalCommand,
                        x => x.CloseMessagesJournalButton)
                    .Do(() => { JournalListBox.ItemsSource = ViewModel.Messages; })
                    .BindCommand(
                        x => x.CopyMessagesToClipboard,
                        x => x.CopyMessagesToClipboardMenuItem,
                        JournalListBox.Events().SelectionChanged
                            .Select(_ => JournalListBox.SelectedItems))
                    .OneWayBind(
                        x => x.Localization["CopyMessage"],
                        x => x.CopyMessagesToClipboardMenuItem.Header)
                    .Do(() => { RoutedViewHost.Router = ViewModel.Router; })
                    ;
            });
        }

        /// <inheritdoc />
        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MainWindowViewModel) value;
        }

        /// <inheritdoc />
        public MainWindowViewModel ViewModel { get; set; }
    }
}