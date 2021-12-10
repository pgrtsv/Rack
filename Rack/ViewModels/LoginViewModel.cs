using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Security.Authentication;
using Npgsql;
using Rack.Localization;
using Rack.Shared;
using Rack.Shared.Configuration;
using Rack.Shared.MainWindow;
using Rack.Shared.Roles;
using ReactiveUI;

namespace Rack.ViewModels
{
    /// <summary>
    /// Модель представления формы аутентификации.
    /// </summary>
    public sealed class LoginViewModel : ReactiveViewModel
    {
        private readonly ObservableAsPropertyHelper<string> _authenticationError;
        
        public LoginViewModel(
            IMainWindowService mainWindowService,
            IDatabaseAuthenticationService databaseAuthenticationService,
            IConfigurationService configurationService,
            ILocalizationService localizationService,
            IScreen hostScreen)
            : base(localizationService, hostScreen)
        {
            MainWindowService = mainWindowService;
            Username = App.Settings.Username;

            AuthentificateCommand = ReactiveCommand.CreateFromTask<string>(
                async (password, cancellationToken) =>
                {
                    var result = await databaseAuthenticationService.AuthenticateAsync(
                        Username, password, cancellationToken);
                    ApplicationPrincipalFacade.Instance.Value = result.Principal;
                    MainWindowService.SendMessage(
                        new Message(
                            string.Format(Localization["WelcomeUsername"], result.Principal.Identity.Name),
                            representationType: RepresentationType.BigMessage));
                    App.Settings.Username = Username;
                    configurationService.SaveConfiguration(App.Settings);
                });

            AuthentificateCommand.ThrownExceptions.Subscribe(exception =>
            {
                MainWindowService.SendMessage(new Message(
                    LocalizeException(exception),
                    MessageType.Error,
                    RepresentationType.BigMessage));
            });

            _authenticationError = this.GetIsActivated()
                .Select(isActivated => isActivated
                    ? this.WhenAnyValue(x => x.Localization, selector: localization =>
                            Observable.Merge(
                                AuthentificateCommand.Select(_ => string.Empty),
                                AuthentificateCommand.ThrownExceptions.Select(LocalizeException)))
                        .Switch()
                    : Observable.Return(string.Empty))
                .Switch()
                .ToProperty(this, nameof(AuthenticationError));

            this.WhenActivated(cleanUp =>
            {
                this.WhenAnyValue(x => x.Localization,
                        localization => localization?["Authentication"] ?? string.Empty)
                    .Do(header => mainWindowService.ChangeHeader(header))
                    .Subscribe()
                    .DisposeWith(cleanUp);
            });
        }

        public IMainWindowService MainWindowService { get; }
        public override IEnumerable<string> LocalizationKeys => new [] {"Rack"};

        public string AuthenticationError => _authenticationError.Value;

        public string Username { get; set; }

        public string LocalizeException(Exception exception)
        {
            return exception switch
            {
                AuthenticationException _ when exception.InnerException is PostgresException postgresException &&
                                               postgresException.SqlState.Equals("28P01")
                => Localization["WrongPasswordOrLogin"],
                AuthenticationException _ when exception.InnerException is NpgsqlException npgsqlException &&
                                               npgsqlException.ErrorCode == -2147467259
                => Localization["NoPasswordProvided"],
                AuthenticationException _ => Localization["ErrorTryAgain"],
                PostgresException postgresException when postgresException.SqlState.Equals("3F000")
                => Localization["AdminSchemaNotFound"],
                PostgresException postgresException when postgresException.SqlState.Equals("42883")
                => Localization["GetUserRolesNotFound"],
                TimeoutException _ => exception.Message,
                SocketException _ => exception.Message,
                _ => exception.Message
            };
        }

        /// <summary>
        /// Производит аутентификацию.
        /// </summary>
        public ReactiveCommand<string, Unit> AuthentificateCommand { get; }
    }
}