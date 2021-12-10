using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using Rack.Localization;
using Rack.Shared;
using Rack.Shared.MainWindow;
using Rack.Shared.Modularity;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Rack.ViewModels
{
    public sealed class ChangelogsViewModel : ReactiveViewModel
    {
        public sealed class ModuleViewModel
        {
            public string ModuleName { get; }
            public string LocalizedName { get; }
            public string Name { get; }

            public ModuleViewModel(string moduleName, string localizedName)
            {
                ModuleName = moduleName;
                LocalizedName = localizedName;
                Name = $"{LocalizedName} ({ModuleName})";
            }

            /// <inheritdoc />
            public override string ToString() => Name;
        }

        private readonly ObservableAsPropertyHelper<string> _selectedChangelog;
        private ModuleViewModel _selectedModule;

        public ChangelogsViewModel(
            IModuleCatalog moduleCatalog,
            IMainWindowService mainWindowService,
            ILocalizationService localizationService,
            IScreen hostScreen)
            : base(localizationService, hostScreen)
        {
            this.WhenAnyValue(
                    x => x.Localization,
                    selector: localization => moduleCatalog.Modules
                        .Select(x => x.Name)
                        .Prepend(App.Name)
                        .Select(module =>
                            new ModuleViewModel(module,
                                LocalizationService.FromAnyLocalization(module)))
                        .AsObservableChangeSet())
                .Switch()
                .Bind(out var modules)
                .Subscribe();
            Modules = modules;
            SelectedModule = modules.First();

            _selectedChangelog = this.WhenAnyValue(x => x.SelectedModule, selectedModule =>
                {
                    if (selectedModule == default) return string.Empty;
                    return File.Exists($@"Changelogs\{selectedModule.ModuleName}.md")
                        ? File.ReadAllText($@"Changelogs\{selectedModule.ModuleName}.md")
                        : string.Empty;
                })
                .ToProperty(this, nameof(SelectedChangelog));

            this.WhenActivated(cleanUp =>
            {
                this.WhenAnyValue(x => x.Localization)
                    .Where(localization => localization != null)
                    .Subscribe(localization =>
                        mainWindowService.ChangeHeader(localization["Changelogs"]))
                    .DisposeWith(cleanUp);
            });
        }

        public ReadOnlyObservableCollection<ModuleViewModel> Modules { get; }

        public ModuleViewModel SelectedModule
        {
            get => _selectedModule;
            set
            {
                if (value != default)
                    this.RaiseAndSetIfChanged(ref _selectedModule, value);
            }
        }

        public string SelectedChangelog => _selectedChangelog.Value;

        public override IEnumerable<string> LocalizationKeys { get; } = new[] {App.Name};
    }
}