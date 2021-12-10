using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
using Rack.LocalizationTool.Models.LocalizationData;
using Rack.LocalizationTool.Models.LocalizationProblem;
using ReactiveUI;

namespace Rack.LocalizationTool.Services
{
    /// <summary>
    /// Сервис для обнаружения проблем, связанных с несогласованностью количества аргументов
    /// в методе форматирования строки (которая использует локализацию)
    /// и количества плейсхолдеров в фразе (фразах) локализации.
    /// </summary>
    public class FormatArgumentsInconsistenciesService: ReactiveObject, IDisposable
    {
        private readonly ProjectLocalizationData _localizationData;
        private readonly CompositeDisposable _cleanUp = new CompositeDisposable();

        private readonly SourceCache<FormatArgumentsInconsistenciesAtFile, string> _stringFormatProblems;

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

        private readonly BehaviorSubject<bool> _isInitialized;
        private readonly BehaviorSubject<bool> _isProblemsDetected;

        public FormatArgumentsInconsistenciesService(ProjectLocalizationData localizationData)
        {
            _localizationData = localizationData;
            _stringFormatProblems = new SourceCache<FormatArgumentsInconsistenciesAtFile, string>(
                x => x.FilePath)
                .DisposeWith(_cleanUp);
            _isInitialized = new BehaviorSubject<bool>(false)
                .DisposeWith(_cleanUp);
            _isProblemsDetected = new BehaviorSubject<bool>(false)
                .DisposeWith(_cleanUp);
        }

        public IObservable<IChangeSet<FormatArgumentsInconsistenciesAtFile, string>> 
            ConnectToFormatArgumentsInconsistencies => _stringFormatProblems.Connect();

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

                _stringFormatProblems.CountChanged
                    .Subscribe(count => mainScheduler.Schedule(() =>
                        _isProblemsDetected.OnNext(count > 0)))
                    .DisposeWith(_cleanUp);

                var localizedFilesDataCache = _localizationData
                    .ConnectToLocalizedFilesData()
                    .AsObservableCache()
                    .DisposeWith(_cleanUp);

                var keyPhrasesCache = _localizationData
                    .ConnectToKeyPhrases()
                    .AsObservableCache()
                    .DisposeWith(_cleanUp);

                localizedFilesDataCache.Connect()
                    .Zip(localizedFilesDataCache.Connect().QueryWhenChanged(),
                        (changeSet, query) => (changeSet, query))
                    .Throttle(TimeSpan.FromSeconds(0.5))
                    .Do(tuple =>
                    {
                        var (changeSet, query) = tuple;
                        _lastLocalizedFileDataCollection = query;
                        if (query.Count == 0 || _lastKeyPhrasesCollection == null ||
                            _lastKeyPhrasesCollection.Count == 0)
                            return;
                        
                        foreach (var change in changeSet)
                            if (change.Reason == ChangeReason.Add || change.Reason == ChangeReason.Update)
                            {
                                if (FileHasFormatArgumentsInconsistency(change.Current, 
                                    _lastKeyPhrasesCollection.Items.ToArray(), out var problem))
                                    _stringFormatProblems.AddOrUpdate(problem);
                                else
                                    _stringFormatProblems.Remove(change.Current.FilePath);
                            }
                            else if (change.Reason == ChangeReason.Remove)
                                _stringFormatProblems.Remove(change.Current.FilePath);
                    })
                    .Subscribe()
                    .DisposeWith(_cleanUp);

                keyPhrasesCache.Connect()
                    .Zip(keyPhrasesCache.Connect().QueryWhenChanged(),
                        (changeSet, query) => (changeSet, query))
                    .Throttle(TimeSpan.FromSeconds(0.5))
                    .Do(tuple =>
                    {
                        var (changeSet, query) = tuple;
                        _lastKeyPhrasesCollection = query;
                        if (query.Count == 0 || _lastLocalizedFileDataCollection == null ||
                            _lastLocalizedFileDataCollection.Count == 0)
                            return;

                        foreach (var localizedFileData in _lastLocalizedFileDataCollection.Items)
                            if (FileHasFormatArgumentsInconsistency(localizedFileData,
                                query.Items.ToArray(), out var problem))
                                _stringFormatProblems.AddOrUpdate(problem);
                            else
                                _stringFormatProblems.Remove(localizedFileData.FilePath);
                    })
                    .Subscribe()
                    .DisposeWith(_cleanUp);

                _isInitialized.OnNext(true);
            });


        public void Dispose()
        {
            _cleanUp?.Dispose();
        }

        /// <summary>
        /// Анализирует, присутствуют ли в файле проблемы с несогласованностью
        /// количества аргументов в методе форматирования строки, и количестве плейсхолдеров.
        /// </summary>
        /// <param name="localizedFileData">Файл использующий локализацию.</param>
        /// <param name="keyPhrases">Список ключей-фраз.</param>
        /// <param name="problem">Выходной параметр, <see lanword="null"/> – если проблем не обнаружено,
        /// иначе экземпляр, представляющий все найденные проблемы в файле.
        /// Если по данному ключу, имеется несогласованность в количествах плейсхолдеров фраз – это тоже
        /// будет считаться за проблему.</param>
        /// <returns><see langword="true"/>, если проблемы обнаружены.</returns>
        public bool FileHasFormatArgumentsInconsistency(
            LocalizedFileData localizedFileData,
            IList<KeyPhrase> keyPhrases,
            out FormatArgumentsInconsistenciesAtFile problem)
        {
            var problemsInFile = new List<FormatArgumentsInconsistency>();
            foreach (var localizedPlace in localizedFileData.LocalizedPlaces)
            {
                var keyPhrase = keyPhrases
                    .FirstOrDefault(x => x.Key == localizedPlace.LocalizationKey);
                if (keyPhrase == null) continue;
                var isPhrasesValid = !PhraseFormatDifferenceService.IsHasStringFormatDifference(keyPhrase);
                if (!isPhrasesValid)
                {
                    problemsInFile.Add(new FormatArgumentsInconsistency(localizedPlace, keyPhrase));
                    continue;
                }

                var placeHoldersCount = keyPhrase.Phrases.First().PlaceHolderCount;
                if(placeHoldersCount != localizedPlace.FormatArguments.Count)
                    problemsInFile.Add(
                        new FormatArgumentsInconsistency(localizedPlace, keyPhrase, placeHoldersCount));
            }

            problem = problemsInFile.Count > 0
                ? new FormatArgumentsInconsistenciesAtFile(problemsInFile, localizedFileData.FilePath,
                    _localizationData.ProjectDirectory)
                : null;
            return problem != null;
        }
    }
}