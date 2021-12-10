using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
using Rack.LocalizationTool.Infrastructure;
using Rack.LocalizationTool.Models.LocalizationData;
using ReactiveUI;

namespace Rack.LocalizationTool.Services
{
    /// <summary>
    /// Сервис для работы с проблемами, связанные с неиспользуемыми ключами-фраз.
    /// </summary>
    public class UnusedKeysService : ReactiveObject, IDisposable
    {
        private readonly ProjectLocalizationData _localizationData;

        private readonly SourceCache<KeyPhrase, string> _unusedKeys;

        private readonly BehaviorSubject<bool> _isInitialized;
        private readonly BehaviorSubject<bool> _isProblemsDetected;

        private readonly CompositeDisposable _cleanUp
            = new CompositeDisposable();

        public UnusedKeysService(ProjectLocalizationData localizationData)
        {
            _localizationData = localizationData;
            _unusedKeys = new SourceCache<KeyPhrase, string>(
                    x => x.Key)
                .DisposeWith(_cleanUp);

            _isInitialized = new BehaviorSubject<bool>(false)
                .DisposeWith(_cleanUp);
            _isProblemsDetected = new BehaviorSubject<bool>(false)
                .DisposeWith(_cleanUp);
        }

        public IObservable<IChangeSet<KeyPhrase, string>> ConnectToUnusedKeys =>
            _unusedKeys.Connect();

        /// <summary>
        /// <see langword="true"/>, если сервис проинициализирован.
        /// </summary>
        public IObservable<bool> IsInitialized => _isInitialized;

        /// <summary>
        /// <see langword="true"/>, если сервис обнаружил проблемы.
        /// <see langword="true"/>, если проблем нет или сервис не проинициализирован.
        /// </summary>
        public IObservable<bool> IsProblemsDetected => _isProblemsDetected;

        /// <summary>
        /// Инициализирует сервис.
        /// </summary>
        public IObservable<Unit> Initialize(IScheduler mainScheduler)
            => Observable.Start(() =>
            {
                if (_isInitialized.FirstAsync().Wait()) return;

                _unusedKeys.CountChanged
                    .Subscribe(count => mainScheduler.Schedule(() => 
                        _isProblemsDetected.OnNext(count > 0)))
                    .DisposeWith(_cleanUp);

                _localizationData.ConnectToKeyPhrases().QueryWhenChanged()
                    .CombineLatest(_localizationData.ConnectToLocalizedFilesData().QueryWhenChanged(),
                        (keyPhrases, localizedFilesData) => (keyPhrases, localizedFilesData))
                    .Throttle(TimeSpan.FromSeconds(0.1))
                    .Subscribe(tuple => 
                        {
                            var (keyPhrases, localizedFilesData) = tuple;
                            mainScheduler.Schedule(() =>
                            {
                                var unusedKeys = FindUnusedKeys(keyPhrases.Items, localizedFilesData.Items)
                                    .ToArray();
                                var deleted = _unusedKeys.Items.Except(unusedKeys);
                                _unusedKeys.AddOrUpdate(unusedKeys);
                                _unusedKeys.Remove(deleted);
                            });
                        })
                    .DisposeWith(_cleanUp);

                _isInitialized.OnNext(true);
            });


        /// <summary>
        /// Удаляет список неиспользуемых ключей-фраз из всех локализаций, где они встречаются.
        /// </summary>
        /// <param name="unusedKeys">Список неиспользуемых ключей для удаления.</param>
        public void RemoveUnusedKeys(IList<KeyPhrase> unusedKeys)
        {
            var forDelete = unusedKeys
                .SelectMany(x => x.GetLocalizations)
                .Distinct()
                .ToDictionary(x => x, x => new List<string>());

            foreach (var unusedKey in unusedKeys)
                foreach (var localizationFile in unusedKey.GetLocalizations)
                    forDelete[localizationFile].Add(unusedKey.Key);

            foreach (var editData in forDelete)
            {
                editData.Key.RewriteKeysPhrases(editData.Key.LocalizedValues
                    .Where(x => !editData.Value.Contains(x.Key))
                    .ToDictionary(x => x.Key, x => x.Value));
            }
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }

        /// <summary>
        /// Совершает поиск неиспользуемых ключей-фраз.
        /// </summary>
        /// <param name="keyPhrases">Все ключи-фраз.</param>
        /// <param name="localizedFileData">Все места локализации.</param>
        /// <returns>Перечисление неиспользуемых ключей-фраз в проекте.</returns>
        private IEnumerable<KeyPhrase> FindUnusedKeys(IEnumerable<KeyPhrase> keyPhrases, 
            IEnumerable<LocalizedFileData> localizedFileData)
        {
            var allUsedKeys = localizedFileData
                .SelectMany(x => x.LocalizedPlaces)
                .Select(x => x.LocalizationKey)
                .Distinct()
                .ToArray();

            return keyPhrases.Where(x => !allUsedKeys.Contains(x.Key));
        }
    }
}