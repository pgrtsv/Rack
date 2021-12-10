using System.Collections.Generic;

namespace Rack.Localization
{
    /// <summary>
    /// Локализация - набор фраз на одном языке.
    /// </summary>
    public interface ILocalization
    {
        /// <summary>
        /// Язык фраз в локализации.
        /// </summary>
        string Language { get; }

        /// <summary>
        /// Ключ локализации.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Ключ-фразы локализации.
        /// </summary>
        IReadOnlyDictionary<string, string> LocalizedValues { get; }
        
        /// <summary>
        /// Возвращает фразу по ключу <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Ключ фразы.</param>
        string this[string key] { get; }
    }
}