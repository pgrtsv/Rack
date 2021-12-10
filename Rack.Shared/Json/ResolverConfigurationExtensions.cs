using System;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json.Serialization;
using Rack.Reflection;
using Rack.Shared.Json.Abstraction;

namespace Rack.Shared.Json
{
    public static class ResolverConfigurationExtensions
    {
        /// <summary>
        /// Проверяет, указано ли свойство как игнорируемое.
        /// </summary>
        /// <param name="hasIgnoredProperties">Экземпляр содержащий данные по игнорируемым свойствам.</param>
        /// <param name="jsonProperty">Свойство (<see cref="JsonProperty" />).</param>
        /// <returns><code>true</code> - если свойство указано, как игнорируемое.</returns>
        public static bool IsIgnored(this IHasIgnoredProperties hasIgnoredProperties, JsonProperty jsonProperty)
        {
            var propertyName = jsonProperty.PropertyName;
            var type = jsonProperty.DeclaringType;
            return hasIgnoredProperties.GlobalIgnoredProperties.Contains(propertyName)
                   || hasIgnoredProperties.TypeIgnoredProperties.ContainsKey(type) &&
                   hasIgnoredProperties.TypeIgnoredProperties[type].Contains(propertyName);
        }

        /// <summary>
        /// Помечает <see cref="JsonProperty" /> как игнорируемое при сериализации свойство,
        /// если это свойство было задано как игнорируемое в экземпляре,
        /// который реализует <see cref="IHasIgnoredProperties" />.
        /// </summary>
        /// <param name="jsonProperty">Свойство (<see cref="JsonProperty" />).</param>
        /// <param name="hasIgnoredProperties">Экземпляр содержащий данные по игнорируемым свойствам.</param>
        /// <returns>Свойство (<see cref="JsonProperty" />).</returns>
        public static JsonProperty IgnoreIfNeeded(this JsonProperty jsonProperty,
            IHasIgnoredProperties hasIgnoredProperties)
        {
            if (!hasIgnoredProperties.IsIgnored(jsonProperty)) return jsonProperty;

            jsonProperty.ShouldSerialize = predicate => false;
            jsonProperty.Ignored = true;

            return jsonProperty;
        }

        /// <summary>
        /// Проверяет, переименуется ли свойство при сериализации.
        /// </summary>
        /// <param name="hasRenamedProperties">
        /// Экземпляр, содержащий данные по свойствам,
        /// которые необходимо переименовать.
        /// </param>
        /// <param name="jsonProperty">Свойство (<see cref="JsonProperty" />).</param>
        /// <returns><code>true</code> - если свойство переименуется ли свойство при сериализации.</returns>
        public static bool IsRenamed(this IHasRenamedProperties hasRenamedProperties, JsonProperty jsonProperty)
        {
            var propertyName = jsonProperty.PropertyName;
            var type = jsonProperty.DeclaringType;
            return hasRenamedProperties.RenamedProperties.ContainsKey(type)
                   && hasRenamedProperties.RenamedProperties[type].Select(x => x.Item1).Contains(propertyName);
        }

        /// <summary>
        /// Переименовывает свойство при сериализации, если это свойство задано для переименования.
        /// </summary>
        /// <param name="jsonProperty">Свойство (<see cref="JsonProperty" />)</param>
        /// <param name="hasRenamedProperties">
        /// Экземпляр, содержащий данные по свойствам,
        /// которые необходимо переименовать.
        /// </param>
        /// <returns>Свойство (<see cref="JsonProperty" />).</returns>
        public static JsonProperty RenameIfNeeded(this JsonProperty jsonProperty,
            IHasRenamedProperties hasRenamedProperties)
        {
            if (!hasRenamedProperties.IsRenamed(jsonProperty)) return jsonProperty;

            jsonProperty.PropertyName = hasRenamedProperties.RenamedProperties[jsonProperty.DeclaringType]
                .First(x => x.Item1 == jsonProperty.PropertyName).Item2;

            return jsonProperty;
        }

        public static IResolverConfiguration<T> IgnorePropertyForAll<T, TProp>(
            this IResolverConfiguration<T> configuration,
            Expression<Func<T, TProp>> expression) where T : class
        {
            return configuration.IgnorePropertyForAll(expression.GetPropertyName());
        }

        public static IResolverConfiguration<T> IgnoreProperty<T, TProp>(this IResolverConfiguration<T> configuration,
            Expression<Func<T, TProp>> expression) where T : class
        {
            return configuration.IgnoreProperty(expression.GetPropertyName());
        }

        public static IResolverConfiguration<T> RenameProperty<T, TProp>(this IResolverConfiguration<T> configuration,
            Expression<Func<T, TProp>> expression, string newName) where T : class
        {
            return configuration.RenameProperty(expression.GetPropertyName(), newName);
        }

        public static ContractResolver<T> ToResolver<T>(this IResolverConfiguration<T> configuration) where T : class
        {
            return new ContractResolver<T>(configuration);
        }
    }
}