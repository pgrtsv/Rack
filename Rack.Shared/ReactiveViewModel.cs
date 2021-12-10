using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Rack.Localization;
using ReactiveUI;

namespace Rack.Shared
{
    public abstract class ReactiveViewModel : ReactiveObject, IRoutableViewModel, IActivatableViewModel
    {
        private readonly ObservableAsPropertyHelper<ILocalization> _localization;
            
        protected ReactiveViewModel(ILocalizationService localizationService, IScreen hostScreen)
        {
            LocalizationService = localizationService;
            HostScreen = hostScreen;

            _localization = 
                this.GetIsActivated()
                    .Select(isActivated =>
                    {
                        if (!isActivated || !LocalizationKeys.Any())
                            return Observable.Return(new EmptyLocalization());
                        if (LocalizationKeys.Count() == 1)
                            return LocalizationService.CurrentLanguage
                                .Select(_ => LocalizationService.GetLocalization(LocalizationKeys.First()));
                        return LocalizationService.CurrentLanguage
                            .Select(_ => new CombinedLocalization(LocalizationKeys.Select(key => LocalizationService.GetLocalization(key))));
                    })
                    .Switch()
                    .ToProperty(this, nameof(Localization), new EmptyLocalization());
        }

        public abstract IEnumerable<string> LocalizationKeys { get; }

        public ILocalizationService LocalizationService { get; }

        public ILocalization Localization => _localization.Value;

        public virtual string UrlPathSegment => GetType().Name;
        public IScreen HostScreen { get; }
        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        public sealed class CombinedLocalization : ILocalization
        {
            private readonly ILocalization[] _localizations;

            public CombinedLocalization(IEnumerable<ILocalization> localizations)
            {
                _localizations = localizations.ToArray();
                Language = _localizations[0].Language;
                Key = _localizations.Aggregate(string.Empty, (s, localization) => s += localization.Key + ' ');

            }

            public string Language { get; }
            public string Key { get; }
            public IReadOnlyDictionary<string, string> LocalizedValues => throw new NotImplementedException();

            public string this[string key] => _localizations.SelectMany(x => x.LocalizedValues)
                .FirstOrDefault(x => x.Key == key)
                .Value;
        }

        public sealed class EmptyLocalization : ILocalization
        {
            public string Language { get; } = string.Empty;
            public string Key { get; } = string.Empty;
            public IReadOnlyDictionary<string, string> LocalizedValues { get; } = new Dictionary<string, string>();

            public string this[string key] => string.Empty;
        }
    }
}