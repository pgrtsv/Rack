using Rack.LocalizationTool.Models.LocalizationData;

namespace Rack.LocalizationTool.Models.LocalizationProblem
{
    /// <summary>
    /// Представляет проблему в файле, связанную с несогласованностью количества аргументов
    /// в методе форматирования строки (которая использует локализацию) и количества плейсхолдеров
    /// в фразе (фразах) локализации.
    /// </summary>
    public class FormatArgumentsInconsistency
    {
        public FormatArgumentsInconsistency(LocalizedPlace localizedPlace, KeyPhrase keyPhrase)
        {
            LocalizedPlace = localizedPlace;
            Key = keyPhrase.Key;
            IsPhrasesFormatValid = false;
            CurrentArgumentsCount = localizedPlace.FormatArguments.Count;
        }

        public FormatArgumentsInconsistency(LocalizedPlace localizedPlace, KeyPhrase keyPhrase, 
            int expectedArgumentsCount)
        {
            LocalizedPlace = localizedPlace;
            Key = keyPhrase.Key;
            IsPhrasesFormatValid = true;
            ExpectedArgumentsCount = expectedArgumentsCount;
            CurrentArgumentsCount = localizedPlace.FormatArguments.Count;
        }

        /// <summary>
        /// Место в файле, где используется локализация, и имеется проблема.
        /// </summary>
        public LocalizedPlace LocalizedPlace { get; }

        /// <summary>
        /// Используемый ключ в проблемном месте <see cref="LocalizedPlace"/>.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// <see langword="true"/>, если количество плейсхолдеров в фразах локализации (по одному ключу) согласовано.
        /// </summary>
        public bool IsPhrasesFormatValid { get; }

        /// <summary>
        /// Количество аргументов, используемое при форматировании строки. 
        /// </summary>
        public int CurrentArgumentsCount { get; }

        /// <summary>
        /// Ожидаемое количество аргументов (количество плейсхолдеров в фразе локализации).
        /// Может быть равным нулю при наличии несогласованности
        /// в фразах локализации <see cref="IsPhrasesFormatValid"/>.
        /// </summary>
        public int ExpectedArgumentsCount { get; }
    }
}