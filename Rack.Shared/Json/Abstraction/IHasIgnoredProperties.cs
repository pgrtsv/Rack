using System;
using System.Collections.Generic;

namespace Rack.Shared.Json.Abstraction
{
    /// <summary>
    /// Интерфейс, представляющий информацию о свойствах, которые необходимо игнорировать при сериализации.
    /// </summary>
    public interface IHasIgnoredProperties
    {
        /// <summary>
        /// Имена свойств, которые будут игнорироваться у всех сущностей в сериализуемом графе.
        /// </summary>
        IReadOnlyCollection<string> GlobalIgnoredProperties { get; }

        /// <summary>
        /// Локальные игнорируемые свойства для определенного типа.
        /// </summary>
        IReadOnlyDictionary<Type, IReadOnlyCollection<string>> TypeIgnoredProperties { get; }
    }
}