using System;
using Newtonsoft.Json.Linq;

namespace Rack.Shared.Configuration
{
    /// <summary>
    /// Сервис, предоставляющий пользовательские настройки.
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        /// Возвращает существующие пользовательские настройки либо пользовательские настройки по умолчанию.
        /// </summary>
        T GetConfiguration<T>() where T : class, IConfiguration;

        /// <summary>
        /// Сохраняет указанные пользовательские настройки.
        /// </summary>
        /// <param name="configuration">Пользовательские настройки.</param>
        void SaveConfiguration<T>(T configuration) where T : class, IConfiguration;

        /// <summary>
        /// Регистрирует конфигурацию по умолчанию.
        /// </summary>
        IConfigurationService RegisterDefaultConfiguration<T>(Func<T> builder)
            where T : class, IConfiguration;

        /// <summary>
        /// Добавляет миграцию.
        /// </summary>
        /// <param name="version">Версия настроек, с которой нужно мигрировать.</param>
        /// <param name="action">Набор действий, которые выполнятся при миграции.</param>
        IConfigurationService AddMigration<T>(Version version, Action<JObject> action)
            where T : class, IConfiguration;
    }
}