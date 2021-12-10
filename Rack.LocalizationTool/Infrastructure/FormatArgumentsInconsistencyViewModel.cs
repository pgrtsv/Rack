using Rack.LocalizationTool.Models.LocalizationData;
using Rack.LocalizationTool.Models.LocalizationProblem;

namespace Rack.LocalizationTool.Infrastructure
{
    /// <summary>
    /// Модель представления проблемы в файле, связанную с несогласованностью количества аргументов
    /// в методе форматирования строки (которая использует локализацию) и количества плейсхолдеров
    /// в фразе (фразах) локализации.
    /// </summary>
    public class FormatArgumentsInconsistencyViewModel
    {
        public FormatArgumentsInconsistencyViewModel(
            FormatArgumentsInconsistency problem,
            FormatArgumentsInconsistenciesAtFile file)
        {
            FilePath = file.FilePath;
            RelativePath = file.RelativePath;
            Key = problem.Key;
            Row = problem.LocalizedPlace.Row;
            CurrentArgumentsCount = problem.CurrentArgumentsCount;
            Arguments = string.Join("; ", problem.LocalizedPlace.FormatArguments);
            ExpectedArgumentsCount = problem.IsPhrasesFormatValid
                ? problem.ExpectedArgumentsCount.ToString() 
                : "?";
            FormatStyle = AsString(problem.LocalizedPlace.FormatStyle);
        }

        /// <summary>
        /// Полный путь к файлу.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Относительный путь к файлу с ошибкой.
        /// </summary>
        public string RelativePath { get; }

        /// <summary>
        /// Номер строки.
        /// </summary>
        public int Row { get; }

        /// <summary>
        /// Ключ фразы.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Аргументы в виде строки: перечисленные в строку через точку с запятой.
        /// </summary>
        public string Arguments { get; }

        /// <summary>
        /// Количество аргументов, используемое при форматировании строки. 
        /// </summary>
        public int CurrentArgumentsCount { get; }

        /// <summary>
        /// Ожидаемое количество аргументов (количество плейсхолдеров в фразе локализации).
        /// Имеет значение "?" при наличии несогласованности
        /// в фразах локализации.
        /// </summary>
        public string ExpectedArgumentsCount { get; }

        /// <summary>
        /// Стиль форматирования значения локализации.
        /// </summary>
        public string FormatStyle { get; }

        /// <summary>
        /// Переводит элемент перечисления <see cref="StringFormatStyle"/> в строку для представления.
        /// </summary>
        /// <param name="formatStyle">Элемент перечисления <see cref="StringFormatStyle"/>.</param>
        /// <returns>Строка.</returns>
        public string AsString(StringFormatStyle formatStyle)
        {
            return formatStyle switch
            {
                StringFormatStyle.WithoutFormat => "Нет",
                StringFormatStyle.ClassicFormat => "string.Format()",
                StringFormatStyle.DefaultFormat => "DefaultFormat()",
            };

        }
    }
}