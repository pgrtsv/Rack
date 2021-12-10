using System.Collections.Generic;
using System.Diagnostics;

namespace Rack.Localization
{
    public sealed class DefaultLocalization : ILocalization
    {
        public DefaultLocalization(string language, string key, IReadOnlyDictionary<string, string> localizedValues)
        {
            LocalizedValues = localizedValues;
            Language = language;
            Key = key;
        }

        /// <inheritdoc />
        public string Language { get; }

        /// <inheritdoc />
        public string Key { get; }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, string> LocalizedValues { get; }

        /// <summary>
        /// Возвращает локализованную фразу по ключу <see cref="key"/>. Если такого ключа нет, возвращает пустую строку.
        /// Не предоставляет отладочной информации.
        /// </summary>
        /// <param name="key">Ключ фразы.</param>
        internal string GetEntry(string key) => LocalizedValues.ContainsKey(key) ? LocalizedValues[key] : string.Empty;

        /// <summary>
        /// Возвращает локализованную фразу по ключу <see cref="key"/>. Если такого ключа нет, возвращает пустую строку.
        /// </summary>
        /// <param name="key">Ключ фразы.</param>
        public string this[string key]
        {
            get
            {
                if (!LocalizedValues.TryGetValue(key, out var entry))
                    Debug.WriteLine(
                        $"Localization error: key \"{key}\" not found in localization \"{Key}\" in \"{Language}\" language.");
                return entry ?? string.Empty;
            }
        }
    }
}