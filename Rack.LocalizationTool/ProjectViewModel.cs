using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using Rack.LocalizationTool.Infrastructure;
using Rack.LocalizationTool.Models.Decorators;
using Rack.LocalizationTool.Models.LocalizationData;
using Rack.LocalizationTool.Models.LocalizationProblem;
using Rack.LocalizationTool.Models.ResolveOptions;
using Rack.LocalizationTool.Services;
using Rack.Shared.BindableDecorators;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Rack.LocalizationTool
{
    public sealed class ProjectViewModel : ReactiveObject, IDisposable
    {
        private readonly CompositeDisposable _cleanUp;

        private readonly ObservableAsPropertyHelper<bool> _isHasAbsentKeys; 
        private readonly ObservableAsPropertyHelper<bool> _isHasUnusedKeysInLocalizations; 
        private readonly ObservableAsPropertyHelper<bool> _isNotExistedKeysUsed;
        private readonly ObservableAsPropertyHelper<bool> _isHasPhraseFormatDifference;
        private readonly ObservableAsPropertyHelper<bool> _isHasFormatArgumentsInconsistencies;

        private readonly ObservableAsPropertyHelper<bool> _isAbsentKeysServiceInitialized;
        private readonly ObservableAsPropertyHelper<bool> _isUnusedKeysServiceInitialized;
        private readonly ObservableAsPropertyHelper<bool> _isNotExistedKeysServiceInitialized;
        private readonly ObservableAsPropertyHelper<bool> _isPhraseFormatDifferenceServiceInitialized;
        private readonly ObservableAsPropertyHelper<bool> _isFormatArgumentsInconsistenciesServiceInitialized;

        public ProjectViewModel(ProjectLocalizationData localizationData)
        {
            _cleanUp = new CompositeDisposable();
            
            ProjectPath = localizationData.ProjectPath;
            ProjectName = Path.GetFileNameWithoutExtension(ProjectPath);

            var unlocalizedStringsService = new UnlocalizedStringsService(localizationData)
                .DisposeWith(_cleanUp);
            var absentKeysService = new AbsentKeysService(localizationData)
                .DisposeWith(_cleanUp);
            var unusedKeysService = new UnusedKeysService(localizationData)
                .DisposeWith(_cleanUp);
            var notExistedKeysService = new NotExistedKeysService(localizationData)
                .DisposeWith(_cleanUp);
            var phraseFormatDifferenceService = new PhraseFormatDifferenceService(localizationData)
                .DisposeWith(_cleanUp);
            var formatArgumentsInconsistenciesService = 
                new FormatArgumentsInconsistenciesService(localizationData)
                    .DisposeWith(_cleanUp);

            unlocalizedStringsService.Initalize(RxApp.MainThreadScheduler)
                .Subscribe()
                .DisposeWith(_cleanUp);

            localizationData.ConnectToLocalizationFiles()
                .Bind(out var localizationFiles)
                .Subscribe()
                .DisposeWith(_cleanUp);
            LocalizationFiles = localizationFiles;

            localizationData.ConnectToKeyPhrases()
                .Sort(SortExpressionComparer<KeyPhrase>.Ascending(x => x.Key))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out var keyPhrases)
                .Subscribe()
                .DisposeWith(_cleanUp);
            KeyPhrases = keyPhrases;

            unlocalizedStringsService.FilesWithUnlocalizedStrings
                .Filter(FilesWithUnlocalizedStringsFilters
                    .Select(x => x.WhenAnyValue(y => y.IsChecked))
                    .Merge()
                    .Select<bool, Func<FileWithUnlocalizedStrings, bool>>(_ => file =>
                        FilesWithUnlocalizedStringsFilters
                            .Where(x => x.IsChecked)
                            .Select(x => x.Filter)
                            .Any(filter => filter.Invoke(file))))
                .RemoveKey()
                .TransformMany(file => file.UnlocalizedStrings.Select(unlocalizedString => new UnlocalizedStringViewModel(unlocalizedString,
                    new MoveUnlocalizedStringOptions(file, unlocalizedString) { LocalizationFile = localizationFiles.FirstOrDefault() },
                    unlocalizedStringsService)))
                .Sort(SortExpressionComparer<UnlocalizedStringViewModel>.Ascending(x =>
                    x.Options.FileWithUnlocalizedString.Name))
                .Bind(out var unlocalizedStrings)
                .Subscribe()
                .DisposeWith(_cleanUp);
            UnlocalizedStrings = unlocalizedStrings;

            localizationData.ConnectToLocalizationFiles()
                .QueryWhenChanged()
                .Throttle(TimeSpan.FromSeconds(0.5))
                .Subscribe(x =>
                {
                    if (UnlocalizedStrings == null || UnlocalizedStrings.Count == 0) return;
                    foreach (var unlocalizedString in UnlocalizedStrings)
                        unlocalizedString.Options.LocalizationFile = x.Items.FirstOrDefault();
                })
                .DisposeWith(_cleanUp);

            absentKeysService.ConnectToAbsentKeys
                .Transform(x => new LocalizationAbsentKeysDecorator(x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out var absentKeys)
                .Subscribe()
                .DisposeWith(_cleanUp);
            AbsentKeys = absentKeys;

            unusedKeysService.ConnectToUnusedKeys
                .Transform(x => new Checked<KeyPhrase>(x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out var unusedKeys)
                .Subscribe()
                .DisposeWith(_cleanUp);
            UnusedKeys = unusedKeys;

            notExistedKeysService.ConnectToAbsentKeys
                .Transform(x => new NotExistedKeysDecorator(x, localizationData.ProjectDirectory))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out var notExistedKeys)
                .Subscribe()
                .DisposeWith(_cleanUp);
            NotExistedKeys = notExistedKeys;

            /* Поскольку при обновлении файла, мы получаем актуальные проблемы (разница форматирования в фразах),
             * посредством новых экземпляров (хотя проблема могла и не изменится), мы не вяжемся к проблемам
             * (и трансформируем их в Модели Представления проблем), а делаем следующую обработку на коннект:
             * при добавлении или удалении – повторяем соответствующие действие с коллекцией
             * Моделей Представлений, а при обновлении – оставляем существующий экземпляр
             * Модели Представлении проблемы, но обновляем его состояние с помощью нового
             * экземпляра проблемы (там могло остаться прежнее состояние,
             * а могло и действительно что-то поменяться, это определит метод обновления). */
            var keyPhraseFormatDifferences = new ObservableCollection<KeyPhraseFormatDifferenceViewModel>();
            phraseFormatDifferenceService.ConnectToStringFormatDifference
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    foreach (var change in x)
                    {
                        var keyPhrase = change.Current;
                        if (change.Reason == ChangeReason.Add)
                            keyPhraseFormatDifferences.Add(new KeyPhraseFormatDifferenceViewModel(keyPhrase));
                        else if (change.Reason == ChangeReason.Update || change.Reason == ChangeReason.Remove)
                        {
                            var existedItem = keyPhraseFormatDifferences
                                .First(differenceViewModel => differenceViewModel.Key == keyPhrase.Key);
                            if (change.Reason == ChangeReason.Update)
                                existedItem.Update(keyPhrase);
                            else
                                keyPhraseFormatDifferences.Remove(existedItem);
                        }
                    }
                })
                .DisposeWith(_cleanUp);
            KeyPhraseFormatDifferences = keyPhraseFormatDifferences;
            keyPhraseFormatDifferences
                .ToObservableChangeSet()
                .AutoRefresh(x => x.IsCheckedForSave)
                .Filter(x => x.IsCheckedForSave)
                .Count()
                .Where(x => x != 0);

            formatArgumentsInconsistenciesService
                .ConnectToFormatArgumentsInconsistencies
                .RemoveKey()
                .TransformMany(file => file.StringFormatProblems
                    .Select(problem => new FormatArgumentsInconsistencyViewModel(problem, file)))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out var formatArgumentsInconsistencies)
                .Subscribe()
                .DisposeWith(_cleanUp);
            FormatArgumentsInconsistencies = formatArgumentsInconsistencies;

            var isLocalizationFilesEditingAvailable = this.WhenAnyObservable(
                    x => x.ResolveAbsentKeysPhrases.IsExecuting,
                    x => x.ApplyPhraseFormatDifferenceSolution.IsExecuting,
                    x => x.DeleteCheckedUnusedKeys.IsExecuting,
                    (a, b, c) => !(a || b || c));

            SetAllOptionsOn = ReactiveCommand.Create(() =>
                    LocalizationProblemsOptions.EnableAll())
                .DisposeWith(_cleanUp);

            SetAllOptionsOff = ReactiveCommand.Create(() =>
                    LocalizationProblemsOptions.DisableAll())
                .DisposeWith(_cleanUp);

            AnalyzeProject = ReactiveCommand.Create( () =>
            {
                if (LocalizationProblemsOptions.IsAnalyzeDifference)
                    absentKeysService.Initialize(RxApp.MainThreadScheduler)
                        .Subscribe()
                        .DisposeWith(_cleanUp);
                if (LocalizationProblemsOptions.IsAnalyzeUnusedKey)
                    unusedKeysService.Initialize(RxApp.MainThreadScheduler)
                        .Subscribe()
                        .DisposeWith(_cleanUp);
                if (LocalizationProblemsOptions.IsAnalyzeNotExistedKey)
                    notExistedKeysService.Initialize(RxApp.MainThreadScheduler)
                        .Subscribe()
                        .DisposeWith(_cleanUp);
                phraseFormatDifferenceService.Initialize(RxApp.MainThreadScheduler)
                    .Subscribe()
                    .DisposeWith(_cleanUp);
                formatArgumentsInconsistenciesService.Initialize(RxApp.MainThreadScheduler)
                    .Subscribe()
                    .DisposeWith(_cleanUp);
            }).DisposeWith(_cleanUp);

            ReplaceNotExistedKeys = ReactiveCommand.Create(() =>
            {
                var replaceOptions = NotExistedKeys
                    .SelectMany(x => x.NotExistedKeyOptions
                        .Where(options => options.IsChecked));
                notExistedKeysService.ReplaceNotExistedKeys(replaceOptions);
            }, notExistedKeysService.IsProblemsDetected)
                .DisposeWith(_cleanUp);

            var isResolveAbsentKeysPhrasesEnabled = absentKeysService.IsProblemsDetected
                .CombineLatest(isLocalizationFilesEditingAvailable, (a, b) => a && b);
            ResolveAbsentKeysPhrases = ReactiveCommand.Create(() =>
            {
                foreach (var absentKeysInLocalization in AbsentKeys)
                {
                    absentKeysInLocalization.LocalizationFile
                        .AddOrUpdateKeysPhrases(absentKeysInLocalization.GetResolvedAbsentKeysPhrases()
                        .ToDictionary(x => x.Key, x => x.Value));
                }
            }, isResolveAbsentKeysPhrasesEnabled)
                .DisposeWith(_cleanUp);

            SetAllUnusedKeysToDelete = ReactiveCommand.Create(() =>
            {
                foreach (var unusedKey in unusedKeys)
                    unusedKey.IsChecked = true;
            }, unusedKeysService.IsProblemsDetected)
                .DisposeWith(_cleanUp);

            SetAllUnusedKeysToSafe = ReactiveCommand.Create(() =>
            {
                foreach (var unusedKey in unusedKeys)
                    unusedKey.IsChecked = false;
            }, unusedKeysService.IsProblemsDetected)
                .DisposeWith(_cleanUp);

            var isDeleteCheckedUnusedKeysEnable = unusedKeysService.IsProblemsDetected
                .CombineLatest(isLocalizationFilesEditingAvailable, 
                    (a, b) => a && b);
            DeleteCheckedUnusedKeys = ReactiveCommand.Create(() =>
            {
                var instancesForDelete = UnusedKeys
                    .Where(x => x.IsChecked)
                    .Select(x => x.Instance)
                    .ToArray();
                unusedKeysService.RemoveUnusedKeys(instancesForDelete);
            }, isDeleteCheckedUnusedKeysEnable)
                .DisposeWith(_cleanUp);

            var isAnyKeyPhraseFormatDifference = keyPhraseFormatDifferences
                .ToObservableChangeSet()
                .AutoRefresh(item => item.IsCheckedForSave)
                .QueryWhenChanged(collection => collection.Any(item => item.IsCheckedForSave));

            var isApplyPhraseFormatDifferenceSolutionEnable = isAnyKeyPhraseFormatDifference
                .CombineLatest(isLocalizationFilesEditingAvailable,
                    (a, b) => a && b);
            ApplyPhraseFormatDifferenceSolution = ReactiveCommand.Create(() =>
            {
                var resolveOptions = keyPhraseFormatDifferences
                    .Where(x => x.IsCheckedForSave
                                && x.Phrases.Select(phrase => phrase.PlaceHolderCount)
                                    .Distinct().Count() == 1)
                    .ToArray();
                var updateInfo = new Dictionary<LocalizationFile, IDictionary<string, string>>();
                foreach (var option in resolveOptions)
                    foreach (var phrase in option.Phrases)
                    {
                        var localizationFile = phrase.LocalizationFile;
                        updateInfo.TryGetValue(localizationFile, out var localizationUpdate);
                        if(localizationUpdate == null)
                        {
                            localizationUpdate = new Dictionary<string, string>();
                            updateInfo.Add(localizationFile, localizationUpdate);
                        }
                        localizationUpdate.Add(option.Key, phrase.Phrase);
                    }

                foreach (var (key, value) in updateInfo)
                    key.AddOrUpdateKeysPhrases(value);
            }, isApplyPhraseFormatDifferenceSolutionEnable)
            .DisposeWith(_cleanUp);

            OpenFile = ReactiveCommand.Create<string>(filePath =>
            {
                Process.Start(new ProcessStartInfo(filePath)
                {
                    UseShellExecute = true
                });
            })
                .DisposeWith(_cleanUp);

            _isHasAbsentKeys = absentKeysService
                .IsProblemsDetected
                .ToProperty(this, x => x.IsHasAbsentKeys)
                .DisposeWith(_cleanUp);

            _isNotExistedKeysUsed = notExistedKeysService
                .IsProblemsDetected
                .ToProperty(this, x => x.IsNotExistedKeysUsed)
                .DisposeWith(_cleanUp);

            _isHasUnusedKeysInLocalizations = unusedKeysService
                .IsProblemsDetected
                .ToProperty(this, x => x.IsHasUnusedKeysInLocalizations)
                .DisposeWith(_cleanUp);

            _isHasPhraseFormatDifference = phraseFormatDifferenceService
                .IsProblemsDetected
                .ToProperty(this, x => x.IsHasPhraseFormatDifference)
                .DisposeWith(_cleanUp);

            _isHasFormatArgumentsInconsistencies = formatArgumentsInconsistenciesService
                .IsProblemsDetected
                .ToProperty(this, x => x.IsHasFormatArgumentsInconsistencies)
                .DisposeWith(_cleanUp);

            _isAbsentKeysServiceInitialized = absentKeysService
                .IsInitialized
                .ToProperty(this, x => x.IsAbsentKeysServiceInitialized)
                .DisposeWith(_cleanUp);

            _isNotExistedKeysServiceInitialized = notExistedKeysService
                .IsInitialized
                .ToProperty(this, x => x.IsNotExistedKeysServiceInitialized)
                .DisposeWith(_cleanUp);

            _isUnusedKeysServiceInitialized = unusedKeysService
                .IsInitialized
                .ToProperty(this, x => x.IsUnusedKeysServiceInitialized)
                .DisposeWith(_cleanUp);

            _isPhraseFormatDifferenceServiceInitialized = phraseFormatDifferenceService
                .IsInitialized
                .ToProperty(this, x => x.IsPhraseFormatDifferenceServiceInitialized)
                .DisposeWith(_cleanUp);

            _isFormatArgumentsInconsistenciesServiceInitialized = formatArgumentsInconsistenciesService
                .IsProblemsDetected
                .ToProperty(this, x => x.IsFormatArgumentsInconsistenciesInitialized)
                .DisposeWith(_cleanUp);
        }

        public ReactiveCommand<Unit, Unit> SetAllOptionsOn { get; }

        public ReactiveCommand<Unit, Unit> SetAllOptionsOff { get; }

        /// <summary>
        /// Анализирует проект на ошибки.
        /// </summary>
        public ReactiveCommand<Unit, Unit> AnalyzeProject { get; }

        /// <summary>
        /// Заменяет те несуществующие в файлах локализаций ключи <see cref="NotExistedKeys"/>,
        /// для которых проинициализированы новые ключи.
        /// </summary>
        public ReactiveCommand<Unit, Unit> ReplaceNotExistedKeys { get; }

        /// <summary>
        /// Добавляет те отсутствующие ключи-фразы <see cref="AbsentKeys"/>
        /// (в соответствующие файлы локализаций), для которых указаны фразы локализации.
        /// </summary>
        [Reactive]
        public ReactiveCommand<Unit, Unit> ResolveAbsentKeysPhrases { get; private set; }

        /// <summary>
        /// Отмечает все неиспользуемые ключей локализаций <see cref="UnusedKeys"/>, как удаляемые.
        /// </summary>
        public ReactiveCommand<Unit, Unit> SetAllUnusedKeysToDelete { get; }

        /// <summary>
        /// Снимает все отметки (на удаления) с неиспользуемых ключей локализаций <see cref="UnusedKeys"/>.
        /// </summary>
        public ReactiveCommand<Unit, Unit> SetAllUnusedKeysToSafe { get; }

        /// <summary>
        /// Удаляет все отмеченные неиспользуемые ключи локализаций
        /// (из файлов, где они встречаются).
        /// </summary>
        [Reactive]
        public ReactiveCommand<Unit, Unit> DeleteCheckedUnusedKeys { get; private set; }

        /// <summary>
        /// Применяет изменения фраз, которые имели различное форматирование (в пределах одного ключа).
        /// </summary>
        [Reactive]
        public ReactiveCommand<Unit, Unit> ApplyPhraseFormatDifferenceSolution { get; private set; }

        /// <summary>
        /// Открывает файл по указанному пути.
        /// </summary>
        public ReactiveCommand<string, Unit> OpenFile { get; }

        /// <summary>
        /// Путь к анализируемому проекту.
        /// </summary>
        public string ProjectPath { get; }

        /// <summary>
        /// Название проекта.
        /// </summary>
        public string ProjectName { get; }

        public LocalizationProblemsOptions LocalizationProblemsOptions { get; }
            = new LocalizationProblemsOptions();

        /// <summary>
        /// Список ключей, со всеми фразами из всех локализаций.
        /// </summary>
        public IReadOnlyCollection<KeyPhrase> KeyPhrases { get; }

        /// <summary>
        /// Актуальный файлы локализаций.
        /// </summary>
        public ReadOnlyObservableCollection<LocalizationFile> LocalizationFiles { get; }

        /// <summary>
        /// Нелокализированные строки.
        /// </summary>
        public ReadOnlyObservableCollection<UnlocalizedStringViewModel> UnlocalizedStrings { get; }

        /// <summary>
        /// Проблемы, связанные с различием множеств ключей между локализациями.
        /// </summary>
        public IReadOnlyCollection<LocalizationAbsentKeysDecorator> AbsentKeys { get; }

        /// <summary>
        /// Неиспользуемые ключи локализации.
        /// </summary>
        public IReadOnlyCollection<Checked<KeyPhrase>> UnusedKeys { get; }

        /// <summary>
        /// Ключи-фразы, которые используется при локализации строк,
        /// но не существуют не в одном файле локализации.
        /// </summary>
        public IReadOnlyCollection<NotExistedKeysDecorator> NotExistedKeys { get; }

        /// <summary>
        /// Проблемы наличия разного количества плейсхолдеров в разных фразах одного ключа.
        /// </summary>
        public IReadOnlyCollection<KeyPhraseFormatDifferenceViewModel> KeyPhraseFormatDifferences { get; }

        /// <summary>
        /// Проблемы несоответствия количества аргументов при форматировании локализированной строки,
        /// и количества плейсхолдеров в соответсвующих фразах.
        /// </summary>
        public IReadOnlyCollection<FormatArgumentsInconsistencyViewModel> FormatArgumentsInconsistencies { get; }

        /// <summary>
        /// <see langword="true"/>, если имеются различия множеств ключей между локализациями.
        /// </summary>
        public bool IsHasAbsentKeys => _isHasAbsentKeys.Value;

        /// <summary>
        /// <see langword="true"/>, если имеются неиспользуемые ключи локализации.
        /// </summary>
        public bool IsHasUnusedKeysInLocalizations => _isHasUnusedKeysInLocalizations.Value;

        /// <summary>
        /// <see langword="true"/>, если в локализации используются несуществующие ключи.
        /// </summary>
        public bool IsNotExistedKeysUsed => _isNotExistedKeysUsed.Value;

        /// <summary>
        /// <see langword="true"/>, если имеется наличия разного количества плейсхолдеров
        /// в разных фразах одного ключа.
        /// </summary>
        public bool IsHasPhraseFormatDifference => _isHasPhraseFormatDifference.Value;

        /// <summary>
        /// <see langword="true"/>, если существуют проблемы несоответствия количества аргументов
        /// при форматировании локализированной строки, и количества плейсхолдеров в соответсвующих фразах.
        /// </summary>
        public bool IsHasFormatArgumentsInconsistencies => _isHasFormatArgumentsInconsistencies.Value;

        /// <summary>
        /// <see langword="true"/>, если сервис по поиску проблем, связанных
        /// с различием множеств ключей между локализациями - проинициализирован.
        /// </summary>
        public bool IsAbsentKeysServiceInitialized => 
            _isAbsentKeysServiceInitialized.Value;

        /// <summary>
        /// <see langword="true"/>, если сервис по поиску неиспользуемых ключей локализации - проинициализирован.
        /// </summary>
        public bool IsUnusedKeysServiceInitialized => 
            _isUnusedKeysServiceInitialized.Value;

        /// <summary>
        /// <see langword="true"/>, если сервис по поиску проблем, связанных
        /// с использованием несуществующих ключей - проинициализирован.
        /// </summary>
        public bool IsNotExistedKeysServiceInitialized => 
            _isNotExistedKeysServiceInitialized.Value;

        /// <summary>
        /// <see langword="true"/>, если сервис по поиску проблем, связанных
        /// наличия разного количества плейсхолдеров в разных фразах одного
        /// ключа - проинициализирован.
        /// </summary>
        public bool IsPhraseFormatDifferenceServiceInitialized =>
            _isPhraseFormatDifferenceServiceInitialized.Value;

        /// <summary>
        /// <see langword="true"/>, если сервис по поиску проблем, связанных
        /// несоответствиtv количества аргументов при форматировании локализированной строки,
        /// и количества плейсхолдеров в соответсвующих фразах - проинициализирован.
        /// </summary>
        public bool IsFormatArgumentsInconsistenciesInitialized =>
            _isFormatArgumentsInconsistenciesServiceInitialized.Value;

        /// <summary>
        /// Фильтры для нелокализированных строк <see cref="UnlocalizedStrings"/>.
        /// </summary>
        public IReadOnlyCollection<FilesFilter> FilesWithUnlocalizedStringsFilters { get; } = new[]
        {
            new FilesFilter("C#", x => Path.GetExtension(x.Path) == ".cs", true),
            new FilesFilter("XAML", x => Path.GetExtension(x.Path) == ".xaml", true)
        };

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}