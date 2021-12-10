using System;
using System.Collections.Generic;

namespace Rack.Shared.Json.Abstraction
{
    /// <summary>
    /// Интерфейс, представляющий информацию о свойствах,
    /// которые необходимо записывать под другим именем при сериализации.
    /// </summary>
    public interface IHasRenamedProperties
    {
        /// <summary>
        /// Свойства, которые необходимо переименовать у экземпляра определенного типа.
        /// У кортежа: первый элемент - это имя свойства в классе типа, второй - имя в JSON-файле.
        /// </summary>
        IReadOnlyDictionary<Type, IReadOnlyCollection<(string, string)>> RenamedProperties { get; }
    }
}