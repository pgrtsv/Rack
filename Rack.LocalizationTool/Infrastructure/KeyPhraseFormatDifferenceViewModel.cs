using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using Rack.LocalizationTool.Models.LocalizationData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Rack.LocalizationTool.Infrastructure
{
    /// <summary>
    /// Модель Представления для разрешения проблемы разного
    /// количество плейсхолдеров у фраз по одному ключу.
    /// </summary>
    public class KeyPhraseFormatDifferenceViewModel: ReactiveObject, IDisposable
    {
        private readonly CompositeDisposable _cleanUp = new CompositeDisposable();
        private SourceCache<LocalizationPhraseViewModel, string> _phrases;

        public KeyPhraseFormatDifferenceViewModel(KeyPhrase key)
        {
            Key = key.Key;
            _phrases = new SourceCache<LocalizationPhraseViewModel, string>(
                x => x.LocalizationFile.FilePath)
                .DisposeWith(_cleanUp);

            _phrases.Connect()
                .ObserveOnDispatcher()
                .Bind(out var phrases)
                .Subscribe()
                .DisposeWith(_cleanUp);
            Phrases = phrases;

            _phrases.Connect()
                .QueryWhenChanged()
                .CombineLatest(_phrases.Connect().WhenPropertyChanged(x => x.PlaceHolderCount), 
                    (query, value) => (query,value))
                .ObserveOnDispatcher()
                .Subscribe(tuple =>
                {
                    var (query, value) = tuple;
                    var isValid = query.Items.Select(x => x.PlaceHolderCount)
                                      .Distinct()
                                      .Count() == 1;
                    IsCanBeCheckedForSave = isValid;
                    if (IsCheckedForSave && !isValid)
                        IsCheckedForSave = false;
                });

            _phrases.AddOrUpdate(key.Phrases.Select(x => new LocalizationPhraseViewModel(x)));
        }

        /// <summary>
        /// Ключ фразы.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Фразы по ключу.
        /// </summary>
        public IReadOnlyCollection<LocalizationPhraseViewModel> Phrases { get; }

        /// <summary>
        /// <see langword="true"/>, если необходимо применить внесенные в фраз изменения.
        /// </summary>
        [Reactive]
        public bool IsCheckedForSave { get; set; }

        /// <summary>
        /// <see langword="true"/>, если можно изменять <see cref="IsCheckedForSave"/>.
        /// </summary>
        [Reactive]
        public bool IsCanBeCheckedForSave { get; private set; }

        /// <summary>
        /// Обновляет ключи фразы на основе актуальной иммутабельной модель ключа и всех его фраз.
        /// </summary>
        /// <param name="update">Ключ и все его фразы.</param>
        public void Update(KeyPhrase update)
        {
            if (Key != update.Key)
                throw new ArgumentException($"Данные в Модели Представления для ключа {Key}, " +
                                            $"не могут быть обновлены с помощью данных ключа {update.Key}.");
            var currentLocalizationPaths = _phrases.Items.Select(x => x.LocalizationFile.FilePath)
                .ToArray();
            var newItems = update.Phrases.Where(x => !currentLocalizationPaths
                    .Contains(x.LocalizationFile.FilePath))
                .Select(x => new LocalizationPhraseViewModel(x));
            var removedItems = currentLocalizationPaths.Except(update.Phrases
                .Select(x => x.LocalizationFile.FilePath));
            var updatedItems = update.Phrases.Where(x => currentLocalizationPaths
                .Contains(x.LocalizationFile.FilePath));
            
            _phrases.Edit(updater =>
            {
                updater.AddOrUpdate(newItems);
                updater.Remove(removedItems);
                foreach (var localizationPhrase in updatedItems)
                    updater.Items
                        .First(x => x.LocalizationFile.FilePath == localizationPhrase.LocalizationFile.FilePath)
                        .UpdateSourcePhrase(localizationPhrase);
            });
        }

        public void Dispose()
        {
            _cleanUp?.Dispose();
        }
    }
}