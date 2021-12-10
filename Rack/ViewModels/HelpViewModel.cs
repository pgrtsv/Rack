using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using Rack.Localization;
using Rack.Navigation;
using Rack.Shared;
using Rack.Shared.Help;
using Rack.Shared.Modularity;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Rack.ViewModels
{
    public sealed class HelpViewModel : ReactiveViewModel, IDialogViewModel
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

        private readonly ObservableAsPropertyHelper<string> _title;

        private readonly ObservableAsPropertyHelper<HelpPage[]>
            _pagesForSelectedModule;

        private ModuleViewModel _selectedModule;

        public HelpViewModel(
            IModuleCatalog moduleCatalog,
            IHelpService helpService,
            ILocalizationService localizationService,
            IScreen hostScreen)
            : base(localizationService, hostScreen)
        {
            var modules = moduleCatalog.Modules
                .Select(x => x.Name)
                .Prepend("Rack")
                .ToArray();
            this.WhenAnyValue(
                    x => x.Localization,
                    selector: localization => modules.Select(module =>
                            new ModuleViewModel(
                                module,
                                localizationService.FromAnyLocalization(module)))
                        .AsObservableChangeSet())
                .Switch()
                .Bind(out var localizedModules)
                .Subscribe();
            Modules = localizedModules;

            _title = this.WhenAnyValue(x => x.Localization,
                    localization => localization?["Help"] ?? string.Empty)
                .ToProperty(this, nameof(Title));

            _pagesForSelectedModule = this.WhenAnyValue(
                    x => x.SelectedModule,
                    selectedModule => selectedModule == null
                        ? new HelpPage[0]
                        : helpService.Pages
                            .Where(page =>
                                page.ModuleName.Equals(selectedModule.ModuleName,
                                    StringComparison.Ordinal))
                            .ToArray())
                .ToProperty(this, nameof(PagesForSelectedModule));

            this.WhenAnyValue(x => x.SelectedModule)
                .Do(module => SelectedPage = PagesForSelectedModule.FirstOrDefault())
                .Subscribe();

            SelectedModule = Modules.First();
        }


        public ReadOnlyObservableCollection<ModuleViewModel> Modules { get; }

        public ModuleViewModel SelectedModule
        {
            get => _selectedModule;
            set
            {
                if (value != null)
                    this.RaiseAndSetIfChanged(ref _selectedModule, value);
            }
        }

        public IReadOnlyCollection<HelpPage> PagesForSelectedModule =>
            _pagesForSelectedModule.Value;

        [Reactive]
        public HelpPage SelectedPage { get; set; }

        public override IEnumerable<string> LocalizationKeys { get; } = new[] {App.Name};
        public bool CanClose { get; } = true;
        public event Action<IReadOnlyDictionary<string, object>> RequestClose;

        public void OnDialogOpened(IReadOnlyDictionary<string, object> parameters)
        {
        }

        public IReadOnlyDictionary<string, object> OnDialogClosed() =>
            new Dictionary<string, object>();

        public string Title => _title.Value;
    }
}