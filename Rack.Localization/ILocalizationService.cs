using System;
using System.Collections.Generic;

namespace Rack.Localization
{
    /// <summary>
    /// Служба локализации.
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Доступные локализации.
        /// </summary>
        IReadOnlyCollection<ILocalization> AvailableLocalizations { get; }
        
        /// <summary>
        /// Доступные языки.
        /// </summary>
        IReadOnlyCollection<string> AvailableLanguages { get; }
        
        /// <summary>
        /// Текущий язык приложения.
        /// </summary>
        IObservable<string> CurrentLanguage { get; }
        
        /// <summary>
        /// Возвращает локализацию по указанному ключу. Если <see cref="language"/>==null, используется <see cref="CurrentLanguage"/>.
        /// </summary>
        /// <param name="key">Ключ локализации.</param>
        /// <param name="language">Язык локализации.</param>
        /// <exception cref="ArgumentException"></exception>
        ILocalization GetLocalization(string key, string language = null);
        
        /// <summary>
        /// Возвращает фразу по указанному ключу фразы из всех доступных локализаций на указанном языке.
        /// </summary>
        /// <param name="phraseKey">Ключ фразы.</param>
        /// <param name="language">Язык, на котором надо найти фразу.</param>
        string FromAnyLocalization(string phraseKey, string language = null);

        /// <summary>
        /// Устанавливает <see cref="CurrentLanguage"/>.
        /// </summary>
        /// <param name="language">Новый язык приложения.</param>
        void SetCurrentLanguage(string language);
    }
}
