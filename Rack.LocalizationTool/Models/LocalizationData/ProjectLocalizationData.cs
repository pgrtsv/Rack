using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DynamicData;
using Rack.LocalizationTool.Services.FileWatcher;
using Rack.Localization;
using ReactiveUI;

namespace Rack.LocalizationTool.Models.LocalizationData
{
    /// <summary>
    /// Информация о локализации в проекте: файлы локализации,
    /// данные о том, где используется локализация.
    /// </summary>
    public sealed class ProjectLocalizationData : ReactiveObject, IDisposable
    {
        private readonly CompositeDisposable _cleanUp = new CompositeDisposable();

        private SourceCache<LocalizedFileData, string> _localizedFileData;
        private SourceCache<KeyPhrase, string> _keyPhrases;

        private readonly LocalizationWatcherService _localizationWatcherService;

        private readonly FileSystemWatcher _csharpFilesWatcher;
        private readonly FileSystemWatcher _xamlFilesWatcher;

        private ProjectLocalizationData(string projectPath)
        {
            if (!File.Exists(projectPath))
                throw new FileNotFoundException("Project not found.", projectPath);
            ProjectPath = projectPath;
            ProjectDirectory = Path.GetDirectoryName(projectPath);
            ProjectName = Path.GetFileNameWithoutExtension(projectPath);

            _localizedFileData = new SourceCache<LocalizedFileData, string>(
                    x => x.FilePath)
                .DisposeWith(_cleanUp);

            _keyPhrases = new SourceCache<KeyPhrase, string>(x => x.Key)
                .DisposeWith(_cleanUp);

            _localizationWatcherService = new LocalizationWatcherService(
                Path.Combine(ProjectDirectory, "Localizations"))
                .DisposeWith(_cleanUp);

            _csharpFilesWatcher = new FileSystemWatcher(ProjectDirectory, "*.cs")
                {
                    IncludeSubdirectories = true
                }
                .DisposeWith(_cleanUp);
            _xamlFilesWatcher = new FileSystemWatcher(ProjectDirectory, "*.xaml")
                {
                    IncludeSubdirectories = true
                }
                .DisposeWith(_cleanUp);

        }

        /// <summary>
        /// Путь к решению.
        /// </summary>
        public string ProjectPath { get; }

        /// <summary>
        /// Путь к директории проекта.
        /// </summary>
        public string ProjectDirectory { get; }

        /// <summary>
        /// Название проекта.
        /// </summary>
        public string ProjectName { get; }

        /// <summary>
        /// Файлы локализации.
        /// </summary>
        public IObservable<IChangeSet<LocalizationFile, string>> ConnectToLocalizationFiles() =>
            _localizationWatcherService.ConnectToLocalizationFiles();

        public IObservable<IChangeSet<LocalizedFileData, string>> ConnectToLocalizedFilesData() =>
            _localizedFileData.Connect();

        public IObservable<IChangeSet<KeyPhrase, string>> ConnectToKeyPhrases() => 
            _keyPhrases.Connect();

        /// <summary>
        /// Возвращает информацию о локализации в указанном проекте.
        /// </summary>
        /// <param name="projectPath">Путь к проекту.</param>
        public static async Task<ProjectLocalizationData> InitProjectLocalizationData(
            string projectPath, IScheduler mainScheduler)
        {
            var ret = new ProjectLocalizationData(projectPath);
            if (!ret.CheckForLocalizationUsage())
                return null;
            await ret.Initialize(mainScheduler).GetAwaiter();
            return ret;
        }

        /// <summary>
        /// Загружает информацию о локализации в решении:
        /// файлы локализации и данные о том, где используется локализация.
        /// </summary>
        public IObservable<Unit> Initialize(IScheduler mainScheduler) => Observable.StartAsync(async () =>
        {
            await _localizationWatcherService.Initialize(mainScheduler).GetAwaiter();

            var filesForAnalyzing = GetFilesForAnalyzing().ToArray();

            _localizationWatcherService.ConnectToLocalizationFiles()
                .QueryWhenChanged()
                .Throttle(TimeSpan.FromSeconds(0.5))
                .Subscribe(query =>
                {
                    var localizations = query.Items.ToArray();
                    mainScheduler.Schedule(() =>
                    {
                        /* Обновляем информацию о фразах для каждого существующего ключа. */
                        foreach (var keyPhrase in _keyPhrases.Items)
                            foreach (var localization in localizations)
                                keyPhrase.HandleLocalization(localization);
                        /* Запоминаем обновленные ключи. */
                        var updateKeyPhrase = _keyPhrases.Items;
                        
                        /* После обновления информации, удаляем те ключи,
                         * которые больше не существуют ни в одной локализации. */
                        var forRemove = _keyPhrases.Items.Where(keyPhrase => !keyPhrase.IsHasElements)
                            .ToArray();
                        if (forRemove.Length > 0)
                        {
                            _keyPhrases.Remove(forRemove);
                            updateKeyPhrase = updateKeyPhrase.Except(forRemove);
                        }
                        
                        /* Указываем подписчикам, что необходимо обработать Refresh
                         * (так как изменилось внутреннее состояние экземпляра) для тех экземпляров,
                         * у которых были обновлены фразы, но после обновления фраз ключи не были
                         * удалены (ещё присутствуют фразы). */
                        _keyPhrases.Refresh(updateKeyPhrase);
                        
                        /* Добавляем новые ключи. */
                        var newKeys = localizations.SelectMany(x => x.LocalizedValues.Keys)
                            .Distinct()
                            .Except(_keyPhrases.Items.Select(x => x.Key));
                        var newKeyPhrases = localizations
                            .SelectMany(localization => localization.LocalizedValues
                                .Where(keyPhrase => newKeys.Contains(keyPhrase.Key))
                                .Select(keyPhrase => (localization, keyPhrase.Key)))
                            .GroupBy(tuple => tuple.Key)
                            .Select(group => new KeyPhrase(
                                group.Key, group.Select(x => x.localization)));

                        _keyPhrases.AddOrUpdate(newKeyPhrases);
                    });
                })
                .DisposeWith(_cleanUp);

            var filesWithLocalization = new List<LocalizedFileData>();
            foreach (var file in filesForAnalyzing)
            {
                var result = await GetFileWithLocalizationOrNullAsync(file);
                if (result != null) filesWithLocalization.Add(result);
            }

            mainScheduler.Schedule(() => _localizedFileData.AddOrUpdate(filesWithLocalization));

            var delay = TimeSpan.FromSeconds(1);
            Observable.FromEventPattern<FileSystemEventArgs>(_csharpFilesWatcher, "Created")
                .Merge(Observable.FromEventPattern<FileSystemEventArgs>(_csharpFilesWatcher, "Changed"))
                .Merge(Observable.FromEventPattern<FileSystemEventArgs>(_xamlFilesWatcher, "Created"))
                .Merge(Observable.FromEventPattern<FileSystemEventArgs>(_xamlFilesWatcher, "Changed"))
                .Delay(delay)
                .Do(pattern => mainScheduler.Schedule(async () =>
                    {
                        var fileName = pattern.EventArgs.FullPath;
                        var result = await GetFileWithLocalizationOrNullAsync(fileName);
                        if(result != null)
                            _localizedFileData.AddOrUpdate(result);
                        else
                            _localizedFileData.Remove(fileName);
                    }
                ))
                .Subscribe()
                .DisposeWith(_cleanUp);

            Observable.FromEventPattern<FileSystemEventArgs>(_csharpFilesWatcher, "Deleted")
                .Merge(Observable.FromEventPattern<FileSystemEventArgs>(_xamlFilesWatcher, "Deleted"))
                .Do(pattern =>
                    mainScheduler.Schedule(() => _localizedFileData.Remove(pattern.EventArgs.FullPath)))
                .Subscribe()
                .DisposeWith(_cleanUp);

            Observable.FromEventPattern<RenamedEventArgs>(_csharpFilesWatcher, "Renamed")
                .Merge(Observable.FromEventPattern<RenamedEventArgs>(_xamlFilesWatcher, "Renamed"))
                .Delay(delay)
                .Do(pattern =>
                {
                    mainScheduler.Schedule(async () =>
                    {
                        _localizedFileData.Remove(pattern.EventArgs.OldFullPath);
                        _localizedFileData.AddOrUpdate(
                            await GetFileWithLocalizationOrNullAsync(pattern.EventArgs.FullPath));
                    });
                })
                .Subscribe()
                .DisposeWith(_cleanUp);

            Observable.FromEventPattern<ErrorEventArgs>(_csharpFilesWatcher, "Error")
                .Merge(Observable.FromEventPattern<ErrorEventArgs>(_xamlFilesWatcher, "Error"))
                .Do(pattern => throw pattern.EventArgs.GetException())
                .Subscribe()
                .DisposeWith(_cleanUp);

            _csharpFilesWatcher.EnableRaisingEvents = true;
            _xamlFilesWatcher.EnableRaisingEvents = true;
        });

        /// <summary>
        /// Инициализирует объект, представляющий данные о файле с локализацией,
        /// если в этом файле присутствует локализация.
        /// </summary>
        /// <param name="fileName">Имя файла, в котором возможно присутсвует локализация.</param>
        /// <returns>
        /// Если в файле присутствует локализация,
        /// вернёт экземпляр <see cref="LocalizedFileData" />, иначе - <code>null</code>.
        /// </returns>
        public async Task<LocalizedFileData> GetFileWithLocalizationOrNullAsync(string fileName)
        {
            var fileLines = await File.ReadAllLinesAsync(fileName);

            const string localizationPattern = "Localization\\[.*\\]";
            var localizationPlaces = new List<LocalizedPlace>();

            for (var i = 0; i < fileLines.Length; i++)
            {
                var line = fileLines[i];
                var matches = Regex.Matches(line, localizationPattern);

                if (matches.Count == 0) continue;

                foreach (Match match in matches)
                {
                    var key = Regex.Replace(
                        match.Value, "(Localization)*\\[*\\\"*\\]*", "");
                    var index = match.Index + match.Value.IndexOf(key);
                    var row = i + 1;
                    /* Далее проверяем строку на наличие форматирования, если оно есть,
                     * значит производим парсинг аргументов. Учитываем как наличие стандартного
                     * форматирования string.Format(), так и альтернативы в виде метода расширения FormatDefault. */
                    var analyzedLine = line.TrimEnd();
                    string[] formatArguments = null;
                    var formatStyle = StringFormatStyle.WithoutFormat;

                    if (IsHasStringDefaultFormat(match, i, fileLines, 
                        out var argumentIndexOut, out var argumentLineIndex))
                    {
                        var argumentIndex = (int)argumentIndexOut;
                        analyzedLine = fileLines[(int) argumentLineIndex];
                        formatArguments = GetFormatArguments(analyzedLine, (int)argumentLineIndex, 
                                argumentIndex, fileLines)
                            .ToArray();
                        formatStyle = StringFormatStyle.DefaultFormat;
                    }

                    else if (IsHasStringClassicFormat(match, i, fileLines))
                    {
                        var argumentIndex = match.Index + match.Length;
                        formatArguments = GetFormatArguments(analyzedLine, i, argumentIndex, fileLines)
                            .ToArray();
                        formatStyle = StringFormatStyle.ClassicFormat;
                    }

                    var localizedPlace = formatArguments == null
                        ? new LocalizedPlace(line.TrimStart(), key, row, index)
                        : new LocalizedPlace(line.TrimStart(), key, row, index, 
                            formatStyle, formatArguments);

                    localizationPlaces.Add(localizedPlace);
                }
            }

            return localizationPlaces.Count > 0
                ? new LocalizedFileData(fileName, localizationPlaces)
                : null;
        }

        public void Dispose()
        {
            _cleanUp?.Dispose();
        }

        /// <summary>
        /// Возвращает <see langword="true"/>, если в проекте используется локализация.
        /// </summary>
        private bool CheckForLocalizationUsage()
        {
            var localizationDirectory = Path.Combine(ProjectDirectory, "Localizations");
            return Directory.Exists(localizationDirectory) && Directory.EnumerateFiles(localizationDirectory).Any();
        }

        /// <summary>
        /// Ищет в директории проекта (включая дочерние директории) все файлы,
        /// которые могут иметь локализацию: расширение «.cs» или «.xaml».
        /// </summary>
        /// <returns>Перечисление путей к файлам, которые могут использовать локализацию.</returns>
        private IEnumerable<string> GetFilesForAnalyzing()
        {
            var isCanBeLocalizedFile = new Func<string, bool>(fileName =>
            {
                var fileExtension = Path.GetExtension(fileName);
                return fileExtension == ".cs" || fileExtension == ".xaml";
            });
            return Directory.EnumerateFiles(ProjectDirectory, "*", SearchOption.AllDirectories)
                .Where(x => isCanBeLocalizedFile(x));
        }

        /// <summary>
        /// Проверяет, является ли локализированное место аргументом метода
        /// <see cref="StringExtensions.FormatDefault"/>
        /// </summary>
        /// <param name="match">Найденное локализированное место.</param>
        /// <param name="matchIndexLine">Строка, в которой найдено локализированное место.</param>
        /// <param name="fileLines">Все строки в файле.</param>
        /// <param name="argumentStartIndex">Выходной параметр: индекс с которого стартует
        /// первый аргумент для форматирования строки.</param>
        /// <param name="argumentLineIndex">Выходной параметр: индекс строки в которой
        /// находится индекс первого аргумента.</param>
        /// <returns><see langword="true"/>, если является.</returns>
        private bool IsHasStringDefaultFormat(Capture match, int matchIndexLine, 
            IReadOnlyList<string> fileLines, 
            out int? argumentStartIndex, out int? argumentLineIndex)
        {
            argumentStartIndex = null;
            argumentLineIndex = null;

            const string checkFormatPattern = ".FormatDefault(";
            var patternIndex = 0;
            var lineCheckIndex = match.Index + match.Length;
            var nextLineIndex = matchIndexLine + 1;
            var checkedLine = fileLines[matchIndexLine];
            while (true)
            {
                /* Если строка закончилась, переходим на следующую. */
                if (lineCheckIndex > checkedLine.Length - 1)
                {
                    checkedLine = fileLines[nextLineIndex++];
                    lineCheckIndex = 0;
                }

                var symbol = checkedLine[lineCheckIndex++];
                if (symbol == ' ' || symbol == '\t' || symbol == '\n') continue;
                /* Встретили отличия от паттерна – значит не FormatDefault. */
                if (symbol != checkFormatPattern[patternIndex++]) return false;
                /* Полностью соврали с паттерном - значит FormatDefault. */
                if (patternIndex > checkFormatPattern.Length - 1)
                {
                    argumentStartIndex = lineCheckIndex;
                    argumentLineIndex = nextLineIndex - 1;
                    return true;
                }
            }
        }

        /// <summary>
        /// Проверяет, является ли локализированное место аргументом метода
        /// <see cref="string.Format(IFormatProvider,string,object)"/>
        /// </summary>
        /// <param name="match">Найденное локализированное место.</param>
        /// <param name="matchIndexLine">Строка, в которой найдено локализированное место.</param>
        /// <param name="fileLines">Все строки в файле.</param>
        /// <returns><see langword="true"/>, если является.</returns>
        private bool IsHasStringClassicFormat(Capture match, int matchIndexLine, 
            IReadOnlyList<string> fileLines)
        {
            const string checkFormatPattern = "string.Format(";
            var patternIndex = checkFormatPattern.Length - 1;
            var lineCheckIndex = match.Index - 1;
            var previousLineIndex = matchIndexLine - 1;
            var checkedLine = fileLines[matchIndexLine];
            while (true)
            {
                if (lineCheckIndex < 0)
                {
                    checkedLine = fileLines[previousLineIndex--];
                    lineCheckIndex = checkedLine.Length - 1;
                }

                var symbol = checkedLine[lineCheckIndex--];
                if (symbol == ' ' || symbol == '\t' || symbol == '\n') continue;
                if (symbol != checkFormatPattern[patternIndex--]) return false;
                if (patternIndex < 0) return true;
            }
        }

        /// <summary>
        /// Возвращает аргументы переданные в метод форматирования строки.
        /// Метод учитывает возможные переносы строки.
        /// </summary>
        /// <param name="line">Строка из файла, содержащая форматирование.</param>
        /// <param name="lineIndex">Индекс этой строки в файле.</param>
        /// <param name="argumentIndex">Индекс начала первого аргумента
        /// (замены для плейсхолдеров).</param>
        /// <param name="fileLines">Все строки из файла.</param>
        /// <returns>Перечисление аргументов,
        /// которые должны быть вставлены вместо плейсхолдеров в строке.</returns>
        private IEnumerable<string> GetFormatArguments(string line, int lineIndex, 
            int argumentIndex, IList<string> fileLines)
        {
            /* true – если достигнут конец метода форматирования. */
            var isEndOfFormat = false;
            /* true – если текущий символ относится к строке. */
            var isQuoteScope = false;
            /* Количество открытых и закрытых скобок, не включая скобок самого метода. */
            var openBracketCount = 0;
            var closeBracketCount = 0;

            /* Хранит считываемый аргумент. */
            var argument = "";
            var symbol = '\0';

            while (!isEndOfFormat)
            {
                /* Если мы дошли до конца строки, а парсинг аргументов ещё не закончен,
                 * значит необходимо продолжить парсинг следующих строк: присоединяем
                 * к аргументу строки следующую строку(строки), причём с учетом того как выполнен разрыва строки. */
                if (argumentIndex >= line.Length - 1)
                {
                    /* Если запятая, то на следующей строке следующий аргумент,
                     * значит просто присоединяем следующую первую не пустую строку. */
                    if (symbol == ',')
                    {
                        var appendLine = fileLines[++lineIndex].Trim();
                        while (string.IsNullOrEmpty(appendLine))
                            appendLine += fileLines[++lineIndex].Trim();
                        line = $"{line} {appendLine}";
                    }

                    else if (symbol == '+')
                    {
                        /* Если на конце «+», нужно проверить, не присутствует ли конкатенация строк. */
                        var reverseIndex = 2;
                        while (argument[^reverseIndex] == ' ')
                            reverseIndex++;

                        if (argument[^reverseIndex] == '"')
                        {
                            isQuoteScope = true;
                            /* Если конкатенация строк, то у текущего считаного аргумента
                             * нужно убрать кавычку и знак «+». */
                            argument = argument.Remove(argument.Length - reverseIndex);
                        }

                        var appendLine = fileLines[++lineIndex].Trim();
                        /* На случай пустых строк. */
                        while (string.IsNullOrEmpty(appendLine))
                            appendLine += fileLines[++lineIndex].Trim();
                        if (isQuoteScope)
                        {
                            /* Необходимо присоединить следующую линию с начала содержания в строке. */
                            while (appendLine[0] != '"')
                                appendLine = appendLine.Remove(0, 1);
                            appendLine = appendLine.Remove(0, 1);
                        }
                        line += isQuoteScope ? appendLine : $" {appendLine}";
                    }

                    else if (symbol == '"')
                    {
                        /* Если на конце «"», нужно проверить, не присутствует ли конкатенация строк.
                         * Для этого нужно посмотреть, нет ли в начале следующей не пустой строки знака «+». */
                        var appendLine = fileLines[++lineIndex].Trim();
                        while (string.IsNullOrEmpty(appendLine))
                            appendLine += fileLines[++lineIndex].Trim();

                        /* Если у нас действительно конкатенация строк, у считываемого аргумента
                         * нужно убрать добавленную кавычку, а следующую строку присоединить
                         * с начала содержания строки. */
                        if (appendLine[0] == '+')
                        {
                            isQuoteScope = true;
                            argument = argument.Remove(argument.Length - 1);
                            /* На случай, если после плюса ещё разрыв строки. */
                            while (!appendLine.Contains("\""))
                                appendLine += fileLines[++lineIndex].Trim();
                            /* Необходимо присоединить следующую линию с начала содержания в строке. */
                            while (appendLine[0] != '"')
                                appendLine = appendLine.Remove(0, 1);
                            appendLine = appendLine.Remove(0, 1);
                        }

                        line += appendLine;
                    }
                    else
                        line += fileLines[++lineIndex].Trim();
                }

                symbol = line[argumentIndex];

                if (symbol == '\"' && line[argumentIndex - 1] != '\\')
                    isQuoteScope = !isQuoteScope;
                else if (symbol == '(' && !isQuoteScope)
                    openBracketCount++;
                else if (symbol == ')' && !isQuoteScope)
                {
                    if (openBracketCount == closeBracketCount)
                        isEndOfFormat = true;
                    else
                        closeBracketCount++;
                }
                
                if (isEndOfFormat)
                {
                    yield return argument != "" ? argument : null;
                    continue;
                }

                if (symbol == ' ' && argument == "")
                {
                    argumentIndex++;
                    continue;
                }
                /* Если встретили запятую, и если мы не находимся внутри строки или выражения,
                 * то только тогда это значит, что мы закончили считывать текущий аргумент. */
                if (symbol == ',' && !isQuoteScope && openBracketCount == closeBracketCount)
                {
                    if (argument == "")
                    {
                        argumentIndex++;
                        continue;
                    }
                    yield return argument;
                    argument = "";
                }
                else
                    argument += symbol;

                argumentIndex++;
            }
        }
    }
}