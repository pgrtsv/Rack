using System;
using System.IO;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
using Newtonsoft.Json;
using Rack.LocalizationTool.Models.LocalizationData;

namespace Rack.LocalizationTool.Services.FileWatcher
{
    using Localization;

    /// <summary>
    /// Сервис для чтения и отслеживания изменений в файлах локализации.
    /// </summary>
    public class LocalizationWatcherService: IDisposable
    {
        private readonly CompositeDisposable _cleanUp = new CompositeDisposable();

        private readonly string _localizationPath;

        private readonly SourceCache<LocalizationFile, string> _localizationFiles;

        private readonly FileSystemWatcher _localizationsWatcher;

        private readonly BehaviorSubject<bool> _isInitialized;

        public LocalizationWatcherService(string localizationPath)
        {
            _localizationPath = localizationPath;
            _localizationFiles = new SourceCache<LocalizationFile, string>(x => x.FilePath)
                .DisposeWith(_cleanUp);
            _localizationsWatcher = new FileSystemWatcher(localizationPath, "*.json")
                .DisposeWith(_cleanUp);
            _isInitialized = new BehaviorSubject<bool>(false)
                .DisposeWith(_cleanUp);
        }

        /// <summary>
        /// <code>true</code>, если сервис инициализирован.
        /// </summary>
        public IObservable<bool> IsInitialized => _isInitialized;

        /// <summary>
        /// Выполняет чтение и запускает отслеживание изменений в файлах локализации.
        /// </summary>
        public IObservable<Unit> Initialize(IScheduler mainScheduler) => Observable.Start(() =>
        {
            if (IsInitialized.FirstAsync().Wait()) return;

            var delay = TimeSpan.FromSeconds(1);

            foreach (var localizationFile in Directory.EnumerateFiles(
                Path.Combine(_localizationPath),
                "*.json"))
                mainScheduler.Schedule(() =>
                    _localizationFiles.AddOrUpdate(new LocalizationFile(localizationFile,
                        JsonConvert.DeserializeObject<DefaultLocalization>(
                            File.ReadAllText(localizationFile)))));

            Observable.FromEventPattern<FileSystemEventArgs>(_localizationsWatcher, "Created")
                .Merge(Observable.FromEventPattern<FileSystemEventArgs>(_localizationsWatcher, "Changed"))
                .Delay(delay)
                .Do(pattern => mainScheduler.Schedule(() =>
                    _localizationFiles.AddOrUpdate(new LocalizationFile(pattern.EventArgs.FullPath,
                        JsonConvert.DeserializeObject<DefaultLocalization>(
                            File.ReadAllText(pattern.EventArgs.FullPath))))))
                .Subscribe()
                .DisposeWith(_cleanUp);

            Observable.FromEventPattern<FileSystemEventArgs>(_localizationsWatcher, "Deleted")
                .Do(pattern =>
                    mainScheduler.Schedule(() => _localizationFiles.Remove(pattern.EventArgs.FullPath)))
                .Subscribe()
                .DisposeWith(_cleanUp);

            Observable.FromEventPattern<RenamedEventArgs>(_localizationsWatcher, "Renamed")
                .Delay(delay)
                .Do(pattern =>
                {
                    mainScheduler.Schedule(() =>
                    {
                        _localizationFiles.Remove(pattern.EventArgs.OldFullPath);
                        _localizationFiles.AddOrUpdate(new LocalizationFile(pattern.EventArgs.FullPath,
                            JsonConvert.DeserializeObject<DefaultLocalization>(
                                File.ReadAllText(pattern.EventArgs.FullPath))));
                    });
                })
                .Subscribe()
                .DisposeWith(_cleanUp);

            Observable.FromEventPattern<ErrorEventArgs>(_localizationsWatcher, "Error")
                .Do(pattern => throw pattern.EventArgs.GetException())
                .Subscribe()
                .DisposeWith(_cleanUp);

            _localizationsWatcher.EnableRaisingEvents = true;
            _isInitialized.OnNext(true);
        });

        /// <summary>
        /// Файлы локализации проекта.
        /// </summary>
        public IObservable<IChangeSet<LocalizationFile, string>> ConnectToLocalizationFiles() =>
            _localizationFiles.Connect();

        public void Dispose()
        {
            _cleanUp?.Dispose();
        }
    }
}