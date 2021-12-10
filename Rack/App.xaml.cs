#if !DEBUG
#define NOT_DEBUG
#endif
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Npgsql;
using Rack.Shared.Configuration;
using Rack.Shared.DataAccess;
using Rack.Shared.Help;
using Rack.Localization;
using Rack.Shared.MainWindow;
using Rack.Shared.Roles;
using Rack.Shared.Updates;
using Rack.ViewModels;
using Rack.Views;
using Serilog;
using Serilog.Exceptions;
using Serilog.Formatting.Json;
using DryIoc;
using Microsoft.Extensions.Configuration;
using Rack.GeoTools.Wpf.Converters;
using Rack.Navigation;
using Rack.Services;
using Rack.Shared;
using Rack.Shared.Modularity;
using Rack.Wpf.Controls;
using ReactiveUI;
using Splat.DryIoc;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Rack
{
    public partial class App : Application
    {
        public const string Name = "Rack";

        public static ApplicationSettings Settings;

        private IConfigurationService _configurationService;
        private Container _container;
        private MainWindowViewModel _mainWindowViewModel;
        private IModuleCatalog _moduleCatalog;


        protected void InitializeModules()
        {
            _moduleCatalog.LoadAndInitializeModules();
        }

        protected void RegisterTypes()
        {
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

            var configurationBuilder = new ConfigurationBuilder();
            if (environment == "Development")
                configurationBuilder
                    .AddJsonFile("appsettings.Development.json", true, true);
            else
                configurationBuilder
                    .AddJsonFile("appsettings.json", false, true);

            var configuration = configurationBuilder.Build();
            _container.RegisterInstance<IConfiguration>(configuration);

            #region Регистрация конвертеров для биндингов ReactiveUI.

            _container.RegisterInstance<IBindingTypeConverter>(
                new BooleanToVisibilityTypeConverter());
            _container.RegisterInstance<IBindingTypeConverter>(
                new ComponentModelTypeConverter());
            _container.RegisterInstance<IBindingTypeConverter>(
                new CustomColorToWpfColorConverter());

            #endregion

            _container.Register<MainWindow>(Reuse.Singleton);
            _container.Register<MainWindowViewModel>(Reuse.Singleton);
            _container.RegisterMapping<IScreen, MainWindowViewModel>();
            _container.Register<ApplicationTabs>(Reuse.Singleton);
            var databaseAuthenticationService = new DatabaseAuthenticationService(configuration);
            _container.RegisterInstance<IDatabaseAuthenticationService>(
                databaseAuthenticationService);
            var notificationListener =
                new DatabaseNotificationListener(databaseAuthenticationService);
            _container.RegisterInstance<IDatabaseNotificationService>(notificationListener);
            _container.Register<DefaultDialogWindow>(Reuse.Transient);
            RegisterForNavigationSingleton<Login, LoginViewModel>();
            RegisterForNavigationSingleton<Help, HelpViewModel>();
            RegisterForNavigationTransient<Changelogs, ChangelogsViewModel>();
            RegisterForNavigationSingleton<AppSettings, AppSettingsViewModel>();
            RegisterForNavigationSingleton<MainMenu, MainMenuViewModel>();
            RegisterForNavigationTransient<YesNo, YesNoViewModel>();
            _container.Register<IModuleCatalog, ModuleCatalog>(Reuse.Singleton);
            _moduleCatalog = _container.Resolve<IModuleCatalog>();
            _container.Register<IDialogService, DialogService>(Reuse.Singleton);
            _container.Register<IMainWindowService, MainWindowService>(Reuse.Singleton);
            _container.Register<IUpdateService, UpdateService>(Reuse.Singleton);

            var helpService = new HelpService();
            helpService.RegisterPage(
                "Общее",
                File.ReadAllText(@"HelpFiles/Rack.Help.General.md"),
                "Rack",
                "Русский");
            _container.RegisterInstance<IHelpService>(helpService);

            _configurationService =
                new ConfigurationService()
                    .RegisterDefaultConfiguration(() => new ApplicationSettings())
                    .AddMigration<ApplicationSettings>(new Version(1, 0), json =>
                    {
                        if (json.ContainsKey("EnabledModules"))
                            json.Remove("EnabledModules");
                    });
            _container.RegisterInstance(_configurationService);

            Settings = _configurationService.GetConfiguration<ApplicationSettings>();

            var localizationService = new JsonLocalizationService();
            localizationService.LoadLocalizations(Settings.Language);
            _container.RegisterInstance<ILocalizationService>(localizationService);

            _container.UseDryIocDependencyResolver();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                Log.Logger = new LoggerConfiguration()
                    .Enrich.WithExceptionDetails()
                    .WriteTo.File(new JsonFormatter(), "logs\\log-.json",
                        rollingInterval: RollingInterval.Day)
                    .CreateLogger();
                base.OnStartup(e);
                _container = new Container();
                RegisterTypes();
                InitializeModules();
                DispatcherUnhandledException += OnDispatcherUnhandledException;
                _mainWindowViewModel = _container.Resolve<MainWindowViewModel>();
                var mainWindow = _container.Resolve<MainWindow>();
                MainWindow = mainWindow;
                mainWindow.ViewModel = _mainWindowViewModel;
                MainWindow.Show();
                _mainWindowViewModel.Router.Navigate
                    .Execute(_container.Resolve<ChangelogsViewModel>())
                    .Subscribe();
            }
            catch (Exception exception)
            {
                var message = "Не удалось запустить программу.";
                Log.Fatal(exception, message);
                throw;
            }
        }

        private void RegisterForNavigationSingleton<TView, TViewModel>()
            where TViewModel : class
            where TView : IViewFor<TViewModel>
        {
            _container.Register<TViewModel>(Reuse.Singleton);
            _container.Register<IViewFor<TViewModel>, TView>(Reuse.Transient);
        }

        private void RegisterForNavigationTransient<TView, TViewModel>()
            where TView : IViewFor<TViewModel> where TViewModel : class
        {
            _container.Register<TViewModel>(Reuse.Transient);
            _container.Register<TView>(Reuse.Transient);
            _container.Register<IViewFor<TViewModel>, TView>(Reuse.Transient);
        }

        private string DataAsString(Exception exception)
        {
            return exception.Data.Keys.Cast<object>()
                .Aggregate(string.Empty,
                    (current, key) => current + $"{key}: {exception.Data[key]}\n");
        }

        private string InnerExceptionsAsString(Exception exception)
        {
            var innerExceptions = string.Empty;
            var innerException = exception.InnerException;
            while (innerException != null)
            {
                innerExceptions += $"{innerException.Message};\n";
                innerException = innerException.InnerException;
            }

            return innerExceptions;
        }

        [Conditional("NOT_DEBUG")]
        private void SendErrorReportToDatabase(Exception exception)
        {
            var connection = _container.Resolve<IDatabaseAuthenticationService>().CurrentConnection;
            connection.ChangeDatabase("Logs");
            using var logCommand = new NpgsqlCommand("INSERT INTO public.\"Exception\" " +
                                                     "(\"Message\", \"StackTrace\", \"Data\". \"Timestamp\", " +
                                                     "\"MachineName\", \"Is64BitOS\", \"OSVersion\", \"InnerExceptions\") " +
                                                     "VALUES (@message, @stacktrace, @data, @timestamp, " +
                                                     "@machinename, @is64bitos, @osversion, @innerexceptions)",
                connection);
            logCommand.Parameters.AddWithValue("message", exception.Message);
            logCommand.Parameters.AddWithValue("stacktrace", exception.StackTrace);
            logCommand.Parameters.AddWithValue("data", DataAsString(exception));
            logCommand.Parameters.AddWithValue("innerexceptions",
                InnerExceptionsAsString(exception));
            logCommand.Parameters.AddWithValue("timestamp", DateTime.UtcNow);
            logCommand.Parameters.AddWithValue("machinename", Environment.MachineName);
            logCommand.Parameters.AddWithValue("is64bitos", Environment.Is64BitOperatingSystem);
            logCommand.Parameters.AddWithValue("osversion", Environment.OSVersion.ToString());

            logCommand.ExecuteNonQuery();
        }

        private void OnDispatcherUnhandledException(object sender,
            DispatcherUnhandledExceptionEventArgs e)
        {
            var exception = e.Exception;
            e.Handled = true;

            if (e.Exception is TaskCanceledException)
                return;


            Log.Fatal(exception, "Было выброшено необработанное исключение.");

            var isReportedToDatabase = false;
            try
            {
                SendErrorReportToDatabase(exception);
#if NOT_DEBUG
                isReportedToDatabase = true;
#endif
            }
            catch
            {
                // ignore
            }

            _container.Resolve<IMainWindowService>().SendMessage(new Message(isReportedToDatabase
                    ? "Возникло непредвиденное исключение, приложение завершает работу. " +
                      "Сообщение об ошибке сохранено в базе данных."
                    : "Возникло непредвиденное исключение, приложение завершает работу.",
                MessageType.Error,
                RepresentationType.BigMessage,
                Shutdown));
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();
        }
    }
}