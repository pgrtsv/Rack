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
using System.Text.RegularExpressions;
using DynamicData;
using Newtonsoft.Json;
using Rack.LocalizationTool.Infrastructure.Comparer;
using Rack.LocalizationTool.Models.LocalizationData;
using Rack.LocalizationTool.Models.LocalizationProblem;
using Rack.LocalizationTool.Models.ResolveOptions;
using Rack.Localization;

namespace Rack.LocalizationTool.Services
{
    /// <summary>
    /// Сервис, предназначенный для обнаружения и исправления нелокализованных строк в проекте C#.
    /// </summary>
    public sealed class UnlocalizedStringsService : IDisposable
    {
        private readonly CompositeDisposable _cleanUp;
        private readonly ProjectLocalizationData _localizationData;
        private readonly FileSystemWatcher _csharpFilesWatcher;
        private readonly FileSystemWatcher _xamlFilesWatcher;
        private readonly SourceCache<FileWithUnlocalizedStrings, string> _filesWithUnlocalizedStrings;
        private readonly BehaviorSubject<bool> _isInitialized;

        public UnlocalizedStringsService(ProjectLocalizationData localizationData)
        {
            _localizationData = localizationData;
            ProjectPath = _localizationData.ProjectPath;
            ProjectDirectory = _localizationData.ProjectDirectory;
            ProjectName = _localizationData.ProjectDirectory;
            
            _cleanUp = new CompositeDisposable();
            _filesWithUnlocalizedStrings = new SourceCache<FileWithUnlocalizedStrings, string>(x => x.Path)
                .DisposeWith(_cleanUp);
            _isInitialized = new BehaviorSubject<bool>(false)
                .DisposeWith(_cleanUp);
            _csharpFilesWatcher = new FileSystemWatcher(ProjectDirectory, "*.cs")
                {
                    IncludeSubdirectories = true
                }
                .DisposeWith(_cleanUp);
            _xamlFilesWatcher = new FileSystemWatcher(ProjectDirectory, "*.xaml")
                    {IncludeSubdirectories = true}
                .DisposeWith(_cleanUp);
        }

        /// <summary>
        /// Путь к файлу проекта C#.
        /// </summary>
        public string ProjectPath { get; }

        /// <summary>
        /// Директория проекта.
        /// </summary>
        public string ProjectDirectory { get; }

        /// <summary>
        /// Название проекта.
        /// </summary>
        public string ProjectName { get; }

        /// <summary>
        /// Найденные нелокализованные строки в проекте.
        /// </summary>
        public IObservable<IChangeSet<FileWithUnlocalizedStrings, string>> FilesWithUnlocalizedStrings =>
            _filesWithUnlocalizedStrings.Connect();

        /// <summary>
        /// <see langword="true"/>, если сервис инициализирован.
        /// </summary>
        public IObservable<bool> IsInitialized => _isInitialized;

        /// <summary>
        /// Все ключи и соответсвующие фразы из файлов локализации.
        /// </summary>
        public IObservable<IChangeSet<KeyPhrase, string>> ConnectToKeyPhrases => 
            _localizationData.ConnectToKeyPhrases();
        
        /// <summary>
        /// Директории, которые следует игнорировать при анализе файлов.
        /// </summary>
        public IReadOnlyCollection<string> IgnoreDirectories { get; } = new[] {"bin", "obj"};

        public void Dispose()
        {
            _cleanUp.Dispose();
        }

        /// <summary>
        /// Заменяет нелокализированное значение значением ключа, который добавляется
        /// в выбранный файл (значение фразы – нелокализированная строка).
        /// </summary>
        /// <param name="options">Параметры перемещения нелокализованных строк в файл локализации.</param>
        public IObservable<Unit> MoveStringToLocalizationFile(MoveUnlocalizedStringOptions options) =>
            Observable.Start(
                () =>
                {
                    var localization = options.LocalizationFile.Localization;
                    localization = new DefaultLocalization(
                        localization.Language, 
                        localization.Key, 
                        localization.LocalizedValues
                        .Append(new KeyValuePair<string, string>(options.Key, options.UnlocalizedString.Value))
                        .ToDictionary(x => x.Key, x => x.Value));
                    File.WriteAllText(
                        options.LocalizationFile.FilePath,
                        JsonConvert.SerializeObject(localization, Formatting.Indented),
                        Encoding.UTF8);

                    var text = File.ReadAllLines(options.FileWithUnlocalizedString.Path);
                    var lineToEdit = text[options.UnlocalizedString.Row];
                    var extension = Path.GetExtension(options.FileWithUnlocalizedString.Path);
                    if (extension == ".cs")
                    {
                        text[options.UnlocalizedString.Row] = lineToEdit.Substring(0, options.UnlocalizedString.Index)
                                                              + ((UnlocalizedCsharpString)options.UnlocalizedString).ToLocalizedFormat(options.Key)
                                                              + lineToEdit.Substring(
                                                                  options.UnlocalizedString.Index +
                                                                  options.UnlocalizedString.String.Length);
                        
                        /* Если строка интерполирована, нам необходимо использовать метод FormatDefault,
                         * поэтому необходимо проверить, существует ли using для namespace-а данного метода,
                         * если нет – то его необходимо добавить.  */
                        if (((UnlocalizedCsharpString) options.UnlocalizedString).IsInterpolated)
                        {
                            var isUsingIncluded = false;
                            var defaultFormatNamespace = "Rack.Shared.Localization";
                            var usingStrings = new List<string>();
                            var getUsingNamespace = new Func<string, string>(line => line
                                .Replace("using", "")
                                .Replace(";", "")
                                .Trim());
                            foreach (var line in text)
                            {
                                if (line.Length < 5 || line.Substring(0, 5) != "using") break;
                                if (getUsingNamespace(line) == defaultFormatNamespace)
                                {
                                    isUsingIncluded = true;
                                    break;
                                }

                                usingStrings.Add(line);
                            }

                            if (!isUsingIncluded)
                            {
                                usingStrings.Add($"using {defaultFormatNamespace};");
                                text = usingStrings
                                    .OrderBy(x => x, new NamespacesComparer())
                                    .Concat(text.TakeLast(text.Length - usingStrings.Count + 1))
                                    .ToArray();
                            }
                        }
                    }
                    else if (extension == ".xaml")
                        text[options.UnlocalizedString.Row] = lineToEdit.Substring(0, options.UnlocalizedString.Index)
                                                              + $"\"{{Binding Localization[{options.Key}]}}\""
                                                              + lineToEdit.Substring(
                                                                  options.UnlocalizedString.Index +
                                                                  options.UnlocalizedString.String.Length);
                    else throw new NotImplementedException();
                    File.WriteAllLines(options.FileWithUnlocalizedString.Path, text, Encoding.UTF8);
                });

        /// <summary>
        /// Заменяет нелокализированное значение существующим ключом.
        /// </summary>
        /// <param name="options">Параметры перемещения нелокализованных строк в файл локализации.</param>
        public IObservable<Unit> ReplaceStringWithExistedKey(MoveUnlocalizedStringOptions options) =>
            Observable.Start(() =>
            {
                var text = File.ReadAllLines(options.FileWithUnlocalizedString.Path);
                var lineToEdit = text[options.UnlocalizedString.Row];
                var extension = Path.GetExtension(options.FileWithUnlocalizedString.Path);
                if (extension == ".cs")
                    text[options.UnlocalizedString.Row] = lineToEdit.Substring(0, options.UnlocalizedString.Index)
                                                          + $"Localization[\"{options.Key}\"]"
                                                          + lineToEdit.Substring(
                                                              options.UnlocalizedString.Index +
                                                              options.UnlocalizedString.String.Length);
                else if (extension == ".xaml")
                    text[options.UnlocalizedString.Row] = lineToEdit.Substring(0, options.UnlocalizedString.Index)
                                                          + $"\"{{Binding Localization[{options.Key}]}}\""
                                                          + lineToEdit.Substring(
                                                              options.UnlocalizedString.Index +
                                                              options.UnlocalizedString.String.Length);
                else throw new NotImplementedException();
                File.WriteAllLines(options.FileWithUnlocalizedString.Path, text, Encoding.UTF8);
            });


        /// <summary>
        /// </summary>
        /// <param name="strings"></param>
        /// <returns></returns>
        public IObservable<Unit> MoveStringsToLocalizationFile(IEnumerable<MoveUnlocalizedStringOptions> strings) =>
            Observable.Start(
                () => { throw new NotImplementedException(); });

        public static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(100);
        
        /// <summary>
        /// Выполняет первоначальный анализ всех .cs и .xaml файлов в <see cref="ProjectDirectory" /> и начинает отслеживание
        /// файлов. При появлении или изменении .cs или .xaml файла в <see cref="ProjectDirectory" /> будет произведён анализ
        /// файла.
        /// </summary>
        public IObservable<Unit> Initalize(IScheduler mainScheduler) => Observable.Start(() =>
        {
            if (IsInitialized.FirstAsync().Wait()) return;

            foreach (var csharpFile in Directory.EnumerateFiles(ProjectDirectory, "*.cs", SearchOption.AllDirectories))
            {
                var fileWithUnlocalizedStrings = AnalyzeCsharpFile(csharpFile);
                if (fileWithUnlocalizedStrings == null) continue;
                mainScheduler.Schedule(() => _filesWithUnlocalizedStrings.AddOrUpdate(fileWithUnlocalizedStrings));
            }

            foreach (var xamlFile in Directory.EnumerateFiles(ProjectDirectory, "*.xaml", SearchOption.AllDirectories))
            {
                var fileWithUnlocalizedStrings = AnalyzeXamlFile(xamlFile);
                if (fileWithUnlocalizedStrings == null) continue;
                mainScheduler.Schedule(() => _filesWithUnlocalizedStrings.AddOrUpdate(fileWithUnlocalizedStrings));
            }

            Observable.FromEventPattern<FileSystemEventArgs>(_csharpFilesWatcher, "Created")
                .Merge(Observable.FromEventPattern<FileSystemEventArgs>(_csharpFilesWatcher, "Changed"))
                .Delay(Delay)
                .Do(pattern =>
                {
                    var newFile = AnalyzeCsharpFile(pattern.EventArgs.FullPath);
                    if (newFile != null)
                        mainScheduler.Schedule(() => _filesWithUnlocalizedStrings.AddOrUpdate(newFile));
                    else
                        mainScheduler.Schedule(() => _filesWithUnlocalizedStrings.Remove(pattern.EventArgs.FullPath));
                })
                .Subscribe()
                .DisposeWith(_cleanUp);

            Observable.FromEventPattern<FileSystemEventArgs>(_csharpFilesWatcher, "Deleted")
                .Do(pattern =>
                {
                    mainScheduler.Schedule(() => _filesWithUnlocalizedStrings.Remove(pattern.EventArgs.FullPath));
                })
                .Subscribe()
                .DisposeWith(_cleanUp);

            Observable.FromEventPattern<RenamedEventArgs>(_csharpFilesWatcher, "Renamed")
                .Delay(Delay)
                .Do(pattern =>
                {
                    var newFile = AnalyzeCsharpFile(pattern.EventArgs.FullPath);
                    mainScheduler.Schedule(() =>
                    {
                        _filesWithUnlocalizedStrings.Remove(pattern.EventArgs.OldFullPath);
                        if (newFile != null)
                            _filesWithUnlocalizedStrings.AddOrUpdate(newFile);
                    });
                })
                .Subscribe()
                .DisposeWith(_cleanUp);

            Observable.FromEventPattern<ErrorEventArgs>(_csharpFilesWatcher, "Error")
                .ObserveOn(mainScheduler)
                .Do(pattern => throw pattern.EventArgs.GetException())
                .Subscribe()
                .DisposeWith(_cleanUp);

            Observable.FromEventPattern<FileSystemEventArgs>(_xamlFilesWatcher, "Created")
                .Merge(Observable.FromEventPattern<FileSystemEventArgs>(_xamlFilesWatcher, "Changed"))
                .Delay(Delay)
                .Do(pattern =>
                {
                    var newFile = AnalyzeXamlFile(pattern.EventArgs.FullPath);
                    if (newFile != null)
                        mainScheduler.Schedule(() => _filesWithUnlocalizedStrings.AddOrUpdate(newFile));
                    else
                        mainScheduler.Schedule(() => _filesWithUnlocalizedStrings.Remove(pattern.EventArgs.FullPath));
                })
                .Subscribe()
                .DisposeWith(_cleanUp);

            Observable.FromEventPattern<FileSystemEventArgs>(_xamlFilesWatcher, "Deleted")
                .Do(pattern =>
                {
                    mainScheduler.Schedule(() => _filesWithUnlocalizedStrings.Remove(pattern.EventArgs.FullPath));
                })
                .Subscribe()
                .DisposeWith(_cleanUp);

            Observable.FromEventPattern<RenamedEventArgs>(_xamlFilesWatcher, "Renamed")
                .Delay(Delay)
                .Do(pattern =>
                {
                    var newFile = AnalyzeXamlFile(pattern.EventArgs.FullPath);
                    mainScheduler.Schedule(() =>
                    {
                        _filesWithUnlocalizedStrings.Remove(pattern.EventArgs.OldFullPath);
                        if (newFile != null)
                            _filesWithUnlocalizedStrings.AddOrUpdate(newFile);
                    });
                })
                .Subscribe()
                .DisposeWith(_cleanUp);

            Observable.FromEventPattern<ErrorEventArgs>(_xamlFilesWatcher, "Error")
                .ObserveOn(mainScheduler)
                .Do(pattern => throw pattern.EventArgs.GetException())
                .Subscribe()
                .DisposeWith(_cleanUp);

            _csharpFilesWatcher.EnableRaisingEvents = true;
            _xamlFilesWatcher.EnableRaisingEvents = true;

            _isInitialized.OnNext(true);
        });

        /// <summary>
        /// Ищет в .cs-файле нелокализованные строки и возвращает информацию о них.
        /// Если файл не содержит нелокализованных строк, возвращает null.
        /// </summary>
        /// <param name="path">Путь к .cs-файлу.</param>
        private FileWithUnlocalizedStrings AnalyzeCsharpFile(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException();
            if (Path.GetExtension(path) != ".cs") throw new FileFormatException();
            using var reader = File.OpenText(path);
            var row = 0;
            var unlocalizedStringsInFile = new List<UnlocalizedCsharpString>();
            var regex = new Regex(@"@?\$?(?<!\\)""([^""]|(?<=\\)"")*\p{IsCyrillic}+([^""]|(?<=\\)"")*(?<!\\)""",
                RegexOptions.Compiled);
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == null || !line.Contains('"'))
                {
                    row++;
                    continue;
                }


                var unlocalizedStrings = regex.Matches(line);
                if (unlocalizedStrings.Count == 0)
                {
                    row++;
                    continue;
                }

                unlocalizedStringsInFile.AddRange(
                    unlocalizedStrings.Select(x => new UnlocalizedCsharpString(x.Value, row, x.Index)));

                row++;
            }

            if (unlocalizedStringsInFile.Count == 0) return null;
            return new FileWithUnlocalizedStrings(path, unlocalizedStringsInFile);
        }

        /// <summary>
        /// Ищет в .xaml-файле нелокализованные строки и возвращает информацию о них.
        /// Если файл не содержит нелокализованных строк, возвращает null.
        /// </summary>
        /// <param name="path">Путь к .xaml-файлу.</param>
        private FileWithUnlocalizedStrings AnalyzeXamlFile(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException();
            if (Path.GetExtension(path) != ".xaml") throw new FileFormatException();
            using var reader = File.OpenText(path);
            var row = 0;
            var unlocalizedStringsInFile = new List<UnlocalizedXamlString>();
            var regex = new Regex(@"(?<!\\)""([^""]|(?<=\\)"")*\p{IsCyrillic}+([^""]|(?<=\\)"")*(?<!\\)""",
                RegexOptions.Compiled);
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == null || !line.Contains('"'))
                {
                    row++;
                    continue;
                }

                var unlocalizedStrings = regex.Matches(line);
                if (unlocalizedStrings.Count == 0)
                {
                    row++;
                    continue;
                }

                unlocalizedStringsInFile.AddRange(
                    unlocalizedStrings.Select(x => new UnlocalizedXamlString(x.Value, row, x.Index)));

                row++;
            }

            if (unlocalizedStringsInFile.Count == 0) return null;
            return new FileWithUnlocalizedStrings(path, unlocalizedStringsInFile);
        }
    }
}