using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using Rack.Shared;
using Rack.Shared.Configuration;
using Rack.Localization;
using Rack.Navigation;
using Rack.Shared.MainWindow;
using Rack.Shared.Modularity;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Rack.ViewModels
{
    public sealed class AppSettingsViewModel : ReactiveViewModel, IDialogViewModel
    {
        private readonly ObservableAsPropertyHelper<ModuleViewModel[]> _modules;
        private readonly ObservableAsPropertyHelper<string> _title;

        public AppSettingsViewModel(
            IMainWindowService mainWindowService,
            IModuleCatalog moduleCatalog,
            IConfigurationService configurationService,
            ILocalizationService localizationService,
            IScreen hostScreen)
            : base(localizationService, hostScreen)
        {
            MainWindowService = mainWindowService;
            AppVersion = Assembly.GetExecutingAssembly().GetName().Version;

            _modules = this.WhenAnyValue(
                    x => x.Localization,
                    selector: localization => moduleCatalog.Modules
                        .Select(module => new ModuleViewModel(
                                module.Name, 
                                localizationService.FromAnyLocalization(module.Name), 
                                module.GetVersion()))
                        .ToArray())
                .ToProperty(this, nameof(Modules));

            AvailableLanguages = LocalizationService.AvailableLanguages;

            _title = this.GetIsActivated()
                .Select(isActivated => isActivated
                    ? this.WhenAnyValue(x => x.Localization, 
                            localization => localization?["Settings"] ?? string.Empty)
                    : Observable.Return(string.Empty))
                .Switch()
                .ToProperty(this, nameof(Title));

            SaveSettings = ReactiveCommand.Create(() =>
            {
                App.Settings.Map(ApplicationSettings);
                LocalizationService.SetCurrentLanguage(ApplicationSettings.Language);
                configurationService.SaveConfiguration(ApplicationSettings);
                MainWindowService.SendMessage(new Message(Localization["SettingsSaved"]));
            });

            ResetSettings = ReactiveCommand.Create(() =>
            {
                App.Settings.Map(new ApplicationSettings());
                ApplicationSettings = App.Settings.Clone();
                configurationService.SaveConfiguration(App.Settings);
                MainWindowService.SendMessage(new Message(Localization["SettingsReset"]));
            });

            this.WhenActivated((CompositeDisposable cleanUp) =>
            {
                ApplicationSettings = App.Settings.Clone();
            });
        }

        public IReadOnlyCollection<string> AvailableLanguages { get; }

        [Reactive] public ApplicationSettings ApplicationSettings { get; set; }

        public ReactiveCommand<Unit, Unit> SaveSettings { get; }
        public ReactiveCommand<Unit, Unit> ResetSettings { get; }

        public IReadOnlyCollection<ModuleViewModel> Modules => _modules.Value;

        public IMainWindowService MainWindowService { get; }

        public Version AppVersion { get; }


        public override IEnumerable<string> LocalizationKeys { get; } = new [] {App.Name};

        public bool CanClose { get; } = true;
        public event Action<IReadOnlyDictionary<string, object>> RequestClose;

        public void OnDialogOpened(IReadOnlyDictionary<string, object> parameters)
        {
        }

        public IReadOnlyDictionary<string, object> OnDialogClosed() => new Dictionary<string, object>();

        public string Title => _title.Value;


        public sealed class ModuleViewModel
        {
            public ModuleViewModel(
                string moduleName, 
                string localizedName,
                Version moduleVersion)
            {
                ModuleName = moduleName;
                ModuleVersion = moduleVersion;
                LocalizedName = localizedName;
                Name = $"{LocalizedName} ({ModuleName} v{ModuleVersion.ToString(3)})";
            }

            /// <summary>
            /// Название модуля.
            /// </summary>
            public string ModuleName { get; }
            
            /// <summary>
            /// Локализованное название модуля.
            /// </summary>
            public string LocalizedName { get; }

            public string Name { get; }

            /// <summary>
            /// Версия модуля.
            /// </summary>
            public Version ModuleVersion { get; }

            /// <inheritdoc />
            public override string ToString() => Name;
        }
    }
}