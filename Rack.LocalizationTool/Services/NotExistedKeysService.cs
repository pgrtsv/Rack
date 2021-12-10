using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using DynamicData;
using Rack.LocalizationTool.Models.LocalizationData;
using Rack.LocalizationTool.Models.LocalizationProblem;
using Rack.LocalizationTool.Models.ResolveOptions;
using ReactiveUI;

namespace Rack.LocalizationTool.Services
{
    /// <summary>
    /// Сервис для работы с проблемами, связанные с использованием,
    /// несуществующих в файлах локализации, ключей-фраз.
    /// </summary>
    public class NotExistedKeysService: ReactiveObject, IDisposable
    {
        private readonly ProjectLocalizationData _localizationData;

        private readonly SourceCache<NotExistedKeys, string> _notExistedKeys;

        private readonly BehaviorSubject<bool> _isInitialized;
        private readonly BehaviorSubject<bool> _isProblemsDetected;

        /// <summary>
        /// Актуальная коллекция файлов использующие локализацию, на момент последнего обработанного
        /// события изменений в файлах использующие локализацию.
        /// </summary>
        private IQuery<LocalizedFileData, string> _lastLocalizedFileDataCollection;

        /// <summary>
        /// Актуальная коллекция ключей локализации, на момент последнего обработанного
        /// события изменения в ключах локализации.
        /// </summary>
        private IQuery<KeyPhrase, string> _lastKeyPhrasesCollection;

        /// <summary>
        /// <see langword="true"/>, если хотя бы раз произведен поиск ошибок.
        /// </summary>
        private bool _isFirstAnalyzeExecuted;

        private readonly CompositeDisposable _cleanUp
            = new CompositeDisposable();

        public NotExistedKeysService(ProjectLocalizationData localizationData)
        {
            _localizationData = localizationData;
            _notExistedKeys = new SourceCache<NotExistedKeys, string>(x => x.FilePath)
                .DisposeWith(_cleanUp);
            _isInitialized = new BehaviorSubject<bool>(false)
                .DisposeWith(_cleanUp);
            _isProblemsDetected = new BehaviorSubject<bool>(false)
                .DisposeWith(_cleanUp);
        }

        /// <summary>
        /// Используемые в проекте ключи-фразы, которые отсутствуют в файлах локализации.
        /// </summary>
        public IObservable<IChangeSet<NotExistedKeys, string>> ConnectToAbsentKeys =>
            _notExistedKeys.Connect();

        /// <summary>
        /// <see langword="true"/>, если сервис проинициализирован.
        /// </summary>
        public IObservable<bool> IsInitialized => _isInitialized;

        /// <summary>
        /// <see langword="true"/>, если сервис обнаружил проблемы.
        /// <see langword="false"/>, если проблем нет или сервис не проинициализирован.
        /// </summary>
        public IObservable<bool> IsProblemsDetected => _isProblemsDetected;

        /// <summary>
        /// Инициализирует сервис.
        /// </summary>
        public IObservable<Unit> Initialize(IScheduler mainScheduler)
            => Observable.Start(() =>
            {
                if (_isInitialized.FirstAsync().Wait()) return;

                _notExistedKeys.CountChanged
                    .Subscribe(count => mainScheduler.Schedule(() =>
                        _isProblemsDetected.OnNext(count > 0)))
                    .DisposeWith(_cleanUp);


                var localizedFilesDataCache = _localizationData
                    .ConnectToLocalizedFilesData()
                    .ObserveOn(mainScheduler)
                    .AsObservableCache()
                    .DisposeWith(_cleanUp);

                var keyPhrasesCache =_localizationData
                    .ConnectToKeyPhrases()
                    .ObserveOn(mainScheduler)
                    .AsObservableCache()
                    .DisposeWith(_cleanUp);

                localizedFilesDataCache.Connect()
                    .ObserveOn(mainScheduler)
                    .Zip(localizedFilesDataCache.Connect().QueryWhenChanged(), 
                        (changeSet, query) => (changeSet, query))
                    .Do(tuple =>  
                    {
                        var (changeSet, query) = tuple;
                        _lastLocalizedFileDataCollection = query;
                        if (query.Count == 0 || _lastKeyPhrasesCollection == null ||
                            _lastKeyPhrasesCollection.Count == 0)
                            return;
                        mainScheduler.Schedule(() => 
                            HandleChangeInLocalizedFiles(changeSet,
                            _lastKeyPhrasesCollection.Items.Select(keyPhrase => keyPhrase.Key).ToArray()));
                        _isFirstAnalyzeExecuted = true;
                    })
                    .Subscribe()
                    .DisposeWith(_cleanUp);
                
                keyPhrasesCache.Connect()
                    .Zip(keyPhrasesCache.Connect().QueryWhenChanged(), 
                        (changeSet, query) => (changeSet, query))
                    .Do(tuple =>
                    {
                        var (changeSet, query) = tuple;
                        _lastKeyPhrasesCollection = query;
                        if (query.Count == 0 || _lastLocalizedFileDataCollection == null ||
                            _lastLocalizedFileDataCollection.Count == 0)
                            return;

                        if (_isFirstAnalyzeExecuted)
                            mainScheduler.Schedule(() => 
                                HandleKeys(changeSet,
                                    _lastLocalizedFileDataCollection.Items.ToArray()));
                        else
                        {
                            var existedKeys = query.Items.Select(x => x.Key).ToArray();
                            mainScheduler.Schedule(() =>
                            {
                                foreach (var localizedFileData in _lastLocalizedFileDataCollection.Items)
                                {
                                    var notExistedKeys = localizedFileData.LocalizedPlaces
                                        .Where(place => !existedKeys.Contains(place.LocalizationKey))
                                        .ToArray();
                                    if (!notExistedKeys.Any()) continue;
                                    _notExistedKeys.AddOrUpdate(
                                        new NotExistedKeys(localizedFileData.FilePath, notExistedKeys));
                                }
                            });

                            _isFirstAnalyzeExecuted = true;
                        }

                    })
                    .Subscribe()
                    .DisposeWith(_cleanUp);

                _isInitialized.OnNext(true);
            });

        /// <summary>
        /// Редактирует в файлах места, где используются несуществующие ключи локализации.
        /// </summary>
        /// <param name="replaceOptionses">Опции замены несуществующих ключей локализации.</param>
        public void ReplaceNotExistedKeys(IEnumerable<ReplaceNotExistedKeyOptions> replaceOptionses)
        {
            if (replaceOptionses == null) 
                throw new ArgumentNullException(nameof(replaceOptionses));

            var groups = replaceOptionses.GroupBy(x => x.FileName);
            foreach (var group in groups)
            {
                var text = File.ReadAllLines(group.Key);
                foreach (var replaceOptions in group)
                {
                    var rowIndex = replaceOptions.LocalizedPlace.Row - 1;
                    // Строка в опциях - без отступов, поэтому берём строку из текста.
                    var fullString = text[rowIndex];
                    var startPart = fullString
                        .Substring(0, replaceOptions.LocalizedPlace.Index);
                    var endPart = fullString.Substring(replaceOptions.LocalizedPlace.Index + 
                                                       replaceOptions.LocalizedPlace.LocalizationKey.Length);
                    text[rowIndex] = $"{startPart}{replaceOptions.NewKey}{endPart}";
                }
                File.WriteAllLines(group.Key, text, Encoding.UTF8);
            }
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }

        /// <summary>
        /// Обрабатывает изменения в файлах проекта.
        /// </summary>
        /// <param name="changeSet">Набор изменений файлов проекта.</param>
        /// <param name="existedKeys">Актуальная на момент изменения коллекция ключей.</param>
        private void HandleChangeInLocalizedFiles(
            IChangeSet<LocalizedFileData, string> changeSet, 
            ICollection<string> existedKeys)
        {
            if (changeSet == null) 
                throw new ArgumentNullException(nameof(changeSet));
            if (existedKeys == null) 
                throw new ArgumentNullException(nameof(existedKeys));
            
            foreach (var change in changeSet)
                switch (change.Reason)
                {
                    /* Обработка добавления и обновления имеет множество пересечений в логике,
                     * поэтому обработка объединена. */
                    case ChangeReason.Update:
                    case ChangeReason.Add:
                    {
                        var current = change.Current;
                        var notExistedKeys = current.LocalizedPlaces
                            .Where(place => !existedKeys.Contains(place.LocalizationKey))
                            .ToArray();
                        var existedItem = _notExistedKeys.Items
                            .FirstOrDefault(item => item.FilePath == current.FilePath);

                        if (existedItem == null && notExistedKeys.Any())
                            _notExistedKeys.AddOrUpdate(new NotExistedKeys(current.FilePath, notExistedKeys));
                        if (existedItem != null)
                        {
                            if (notExistedKeys.Any())
                                existedItem.UpdateNotExistedKeys(notExistedKeys);
                            else
                                _notExistedKeys.Remove(existedItem);
                        }
                        break;
                    }
                    case ChangeReason.Remove:
                        _notExistedKeys.Remove(change.Key);
                        break;
                }
        }

        /// <summary>
        /// Обрабатывает изменения в ключах локализации.
        /// </summary>
        /// <param name="changeSet">Набор изменений в ключах локализации.</param>
        /// <param name="localizedFilesData">Коллекция данных по файлам использующие локализацию,
        /// актуальная на момент события изменения ключей.</param>
        private void HandleKeys(
            IChangeSet<KeyPhrase, string> changeSet,
            IEnumerable<LocalizedFileData> localizedFilesData)
        {
            if (changeSet == null) 
                throw new ArgumentNullException(nameof(changeSet));
            if (localizedFilesData == null) 
                throw new ArgumentNullException(nameof(localizedFilesData));
            
            var addedKeys = changeSet
                .Where(change => change.Reason == ChangeReason.Add)
                .Select(change => change.Current.Key)
                .ToArray();
            var removedKeys = changeSet
                .Where(change => change.Reason == ChangeReason.Remove)
                .Select(change => change.Current.Key)
                .ToArray();

            if (addedKeys.Any())
            {
                foreach (var item in _notExistedKeys.Items)
                {
                    item.HandleAddingKeys(addedKeys);
                    if (!item.IsContainsProblems())
                        _notExistedKeys.Remove(item);
                }
            }

            if (!removedKeys.Any()) return;

            /* Ищем где встречаются удаленные ключ. */
            var newNotExistedKeys = localizedFilesData.Select(localizedFileData =>
                (
                    localizedFileData.FilePath,
                    localizedFileData.LocalizedPlaces
                        .Where(localizedPlace =>
                            removedKeys.Contains(localizedPlace.LocalizationKey))
                        .ToArray()
                ))
                .Where(tuple => tuple.Item2.Any())
                .ToArray();

            if (!newNotExistedKeys.Any()) return;

            /* Смотрим, если среди обнаруженных проблем есть те проблемы, которые относятся
             * к файлам присутствующим в коллекции, то добавляем новую информацию по отсутствующим
             * ключам для этих элементов коллекции; иначе добавляем новый элемент в коллекцию. */
            foreach (var updateInfo in newNotExistedKeys)
            {
                var existedItem = _notExistedKeys.Items
                    .FirstOrDefault(item => item.FilePath == updateInfo.FilePath);
                if (existedItem != null)
                    existedItem.AddNotExistedKeys(updateInfo.Item2);
                else
                    _notExistedKeys.AddOrUpdate(
                        new NotExistedKeys(updateInfo.FilePath, updateInfo.Item2));
            }
        }
    }
}