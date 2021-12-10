using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using Rack.ViewModels;
using ReactiveUI;

namespace Rack.Views
{
    public partial class Login : ReactiveUserControl<LoginViewModel>
    {
        public Login()
        {
            InitializeComponent();
            RememberMeCheckBox.IsEnabled = false;
            this.WhenActivated(cleanUp =>
            {
                this.Bind(ViewModel, x => x.Username, x => x.UsernameTextBox.Text)
                    .DisposeWith(cleanUp);
                this.BindCommand(
                    ViewModel,
                    x => x.AuthentificateCommand,
                    x => x.LoginButton,
                    Observable.FromEventPattern<RoutedEventHandler, EventArgs>(
                            handler => PasswordBox.PasswordChanged += handler,
                            handler => PasswordBox.PasswordChanged -= handler)
                        .Select(_ => PasswordBox.Password));
                ViewModel.AuthentificateCommand.IsExecuting
                    .Do(isAuthenticating => LoginProgressBar.Visibility =
                        isAuthenticating ? Visibility.Visible : Visibility.Collapsed)
                    .Subscribe()
                    .DisposeWith(cleanUp);
                this.OneWayBind(ViewModel, x => x.Localization["ToLogin"], x => x.LoginButton.Content)
                    .DisposeWith(cleanUp);
                this.OneWayBind(ViewModel, x => x.AuthenticationError, x => x.ErrorTextBlock.Text)
                    .DisposeWith(cleanUp);
                this.OneWayBind(ViewModel, x => x.Localization["Login"], x => x.UsernameHintTextBlock.Text)
                    .DisposeWith(cleanUp);
                this.OneWayBind(ViewModel, x => x.Localization["Password"], x => x.PasswordHintTextBlock.Text)
                    .DisposeWith(cleanUp);
                this.OneWayBind(ViewModel, x => x.Localization["RememberMe"], x => x.RememberMeCheckBox.Content)
                    .DisposeWith(cleanUp);
            });
        }
    }
}