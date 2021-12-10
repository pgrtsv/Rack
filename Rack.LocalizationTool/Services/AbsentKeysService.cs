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
    /// Сервис для работы с проблемами, связанные с отсутствием ключей-фраз,
    /// которые присутствуют в других файлах локализации.
    /// </summary>
    public class AbsentKeysService: ReactiveObject, IDisposable
    {
        private readonly ProjectLocalizationData _localizationData;

        private readonly SourceCache<LocalizationAbsentKeys, string> _absentKeys;

        private readonly BehaviorSubject<bool> _isInitialized;
        private readonly BehaviorSubject<bool> _isProblemsDetected;

        private readonly CompositeDisposable _cleanUp
            = new CompositeDisposable();

        public AbsentKeysService(ProjectLocalizationData localizationData)
        {
            _localizationData = localizationData;
            _absentKeys = new SourceCache<LocalizationAbsentKeys, string>(
                x => x.LocalizationFile.FilePath)
                .DisposeWith(_cleanUp);
            _isInitialized = new BehaviorSubject<bool>(false)
                .DisposeWith(_cleanUp);
            _isProblemsDetected = new BehaviorSubject<bool>(false)
                .DisposeWith(_cleanUp);
        }

        /// <summary>
        /// Отсутствующие ключи в файлах локализации.
        /// </summary>
        public IObservable<IChangeSet<LocalizationAbsentKeys, string>> ConnectToAbsentKeys =>
            _absentKeys.Connect();


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
            if(_isInitialized.FirstAsync().Wait()) return;

            _absentKeys.CountChanged
                .Subscribe(count => mainScheduler.Schedule(() =>
                    _isProblemsDetected.OnNext(count > 0)))
                .DisposeWith(_cleanUp);

            _localizationData.ConnectToLocalizationFiles()
                .QueryWhenChanged()
                .Throttle(TimeSpan.FromSeconds(0.1))
                .Subscribe(x =>
                {
                    var localizationFiles = x.Items.ToArray();
                    mainScheduler.Schedule(() => 
                    {
                        foreach (var localizationFile in localizationFiles)
                        {
                            var item = InitializeAbsentKeys(localizationFile, localizationFiles);
                            if (item.AbsentKeys.Count > 0)
                                _absentKeys.AddOrUpdate(item);
                            else
                                _absentKeys.Remove(item);
                        }
                    });
                    
                }).DisposeWith(_cleanUp);

            _isInitialized.OnNext(true);
        });

        public void Dispose()
        {
            _cleanUp.Dispose();
        }

        /// <summary>
        /// Инициализирует данные об отсутствующих ключах в файле локализации.
        /// </summary>
        /// <param name="localization">Файл локализации.</param>
        /// <param name="localizationFiles">Перечисление всех файлов локализации.</param>
        /// <returns>Данные об отсутствующих ключах.</returns>
        private LocalizationAbsentKeys InitializeAbsentKeys(LocalizationFile localization, 
            IEnumerable<LocalizationFile> localizationFiles)
        {
            var absentKeys = new List<string>();
            foreach (var localizationFile in localizationFiles)
            {
                if (localizationFile == localization) continue;

                absentKeys.AddRange(
                    localizationFile.LocalizedValues.Keys
                        .Except(localization.LocalizedValues.Keys));
            }

            return new LocalizationAbsentKeys(localization, absentKeys.Distinct());
        }
    }
}