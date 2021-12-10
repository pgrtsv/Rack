using System;
using System.Text.RegularExpressions;

namespace Rack.LocalizationTool.Models.LocalizationData
{
    /// <summary>
    /// Фраза локализации.
    /// </summary>
    public class LocalizationPhrase
    {
        private static readonly Regex PlaceHolderRegex = new Regex(@"{\d}");

        public LocalizationPhrase(KeyPhrase key, LocalizationFile localizationFile)
        {
            LocalizationFile = localizationFile;
            if (!localizationFile.LocalizedValues.TryGetValue(key.Key, out var phrase))
                throw new ArgumentException($"В локализации {localizationFile.FileName} " +
                                            $"не содержится фразы по ключу {key.Key}.");
            Phrase = phrase;
            var matches = PlaceHolderRegex.Matches(Phrase);
            PlaceHolderCount = matches.Count;
            IsFormatted = PlaceHolderCount > 0;
            Key = key.Key;
        }

        /// <summary>
        /// Файл локализации, который содержит данную фразу.
        /// </summary>
        public LocalizationFile LocalizationFile { get; }

        /// <summary>
        /// Фраза.
        /// </summary>
        public string Phrase { get; }

        /// <summary>
        /// Направлена на применение форматирования: есть плейсхолдеры.
        /// </summary>
        public bool IsFormatted { get; }

        /// <summary>
        /// Количество плейсхолдеров.
        /// </summary>
        public int PlaceHolderCount { get; }

        /// <summary>
        /// Ключ фразы.
        /// </summary>
        public string Key { get; }
    }
}