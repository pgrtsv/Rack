using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using Newtonsoft.Json;

namespace Rack.Localization
{
    public sealed class JsonLocalizationService : ILocalizationService, IDisposable
    {
        private readonly BehaviorSubject<string> _currentLanguage;
        private string[] _availableLanguages;
        private DefaultLocalization[] _localizations;

        public JsonLocalizationService() => _currentLanguage = new BehaviorSubject<string>(DefaultLanguage);

        public string DefaultLanguage => "Русский";

        public IReadOnlyCollection<ILocalization> AvailableLocalizations => _localizations;
        public IReadOnlyCollection<string> AvailableLanguages => _availableLanguages;

        public IObservable<string> CurrentLanguage => _currentLanguage;

        public ILocalization GetLocalization(string key, string language = null)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(language)) language = _currentLanguage.Value;
            return _localizations.FirstOrDefault(x => x.Key.Equals(key) && x.Language.Equals(language)) ??
                   _localizations.First(x => x.Key.Equals(key) && x.Language.Equals(DefaultLanguage));
        }

        public string FromAnyLocalization(string key, string language = null)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(language)) language = _currentLanguage.Value;
#if DEBUG
            var foundEntries = _localizations
                .Where(x => x.Language.Equals(language))
                .Select(x => (localizationKey: x.Key, entry: x.GetEntry(key)))
                .Where(x => !string.IsNullOrEmpty(x.entry))
                .ToArray();
            if (foundEntries.Length == 0)
            {
                Debug.WriteLine(
                    $"Localization error: key \"{key}\" was not found in any localization of language \"{language}\".");
                return string.Empty;
            }

            if (foundEntries.Length > 1)
            {
                Debug.WriteLine(
                    $"Localization error: key \"{key}\" was found in several localizations of language \"{language}\": {foundEntries.Aggregate(string.Empty, (acc, x) => acc == string.Empty ? x.localizationKey : $"{acc}, {x.localizationKey}")}");
                return foundEntries[0].entry;
            }

            return foundEntries[0].entry;
#else
            return _localizations
                .Where(x => x.Language.Equals(language))
                .Select(x => x[key])
                .FirstOrDefault(x => !string.IsNullOrEmpty(x)) ?? string.Empty;
#endif
        }

        public void SetCurrentLanguage(string language)
        {
            if (!_availableLanguages.Contains(language))
                throw new ArgumentException("Provided language is unavailable.", nameof(language));
            _currentLanguage.OnNext(language);
        }

        public void Dispose() => _currentLanguage.Dispose();

        public void LoadLocalizations(string defaultLanguage = null)
        {
            var files = Directory.GetFiles("Localizations", "*.json");
            _localizations = new DefaultLocalization[files.Length];
            for (var i = 0; i < files.Length; i++)
                _localizations[i] = JsonConvert.DeserializeObject<DefaultLocalization>(File.ReadAllText(files[i]));
            _availableLanguages = _localizations.Select(x => x.Language).Distinct().ToArray();
            if (string.IsNullOrWhiteSpace(defaultLanguage))
                defaultLanguage = DefaultLanguage;
            if (!_availableLanguages.Contains(defaultLanguage))
                throw new LocalizationServiceException("Not single one localization in russian language is available.");
            _currentLanguage.OnNext(defaultLanguage);
        }
    }
}