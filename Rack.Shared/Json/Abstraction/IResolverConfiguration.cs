namespace Rack.Shared.Json.Abstraction
{
    /// <summary>
    /// Конфигурация сериализации типа в JSON.
    /// </summary>
    /// <typeparam name="T">Сериализуемый тип.</typeparam>
    public interface IResolverConfiguration<T> : IHasIgnoredProperties, IHasRenamedProperties
        where T : class
    {
        /// <summary>
        /// Игнорирует свойство.
        /// </summary>
        /// <param name="propertyName">Имя свойства.</param>
        /// <returns>Конфигурация.</returns>
        IResolverConfiguration<T> IgnoreProperty(string propertyName);

        /// <summary>
        /// Игнорирует свойство во всем сериализуемом графе.
        /// </summary>
        /// <param name="propertyName">Имя свойства.</param>
        /// <returns>Конфигурация.</returns>
        IResolverConfiguration<T> IgnorePropertyForAll(string propertyName);

        /// <summary>
        /// Устанавливает сериализуемое имя в JSON-файле для указанного свойства.
        /// </summary>
        /// <typeparam name="T">Тип.</typeparam>
        /// <param name="propertyName">Имя свойства в классе.</param>
        /// <param name="jsonFileName">Имя свойства в JSON-файле.</param>
        /// <returns>Конфигурация.</returns>
        IResolverConfiguration<T> RenameProperty(string propertyName, string jsonFileName);

        /// <summary>
        /// Объединяет конфигурации.
        /// </summary>
        /// <typeparam name="TAnother">Сериализуемый тип другой конфигурации.</typeparam>
        /// <param name="configuration">Другая конфигурация.</param>
        /// <returns>Текущая конфигурация.</returns>
        IResolverConfiguration<T> Merge<TAnother>(IResolverConfiguration<TAnother> configuration)
            where TAnother : class;
    }
}