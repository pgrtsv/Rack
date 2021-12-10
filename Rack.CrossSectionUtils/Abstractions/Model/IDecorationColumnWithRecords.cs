using System.Collections.Generic;

namespace Rack.CrossSectionUtils.Abstractions.Model
{
    /// <summary>
    /// Колонка оформления разреза с коллекцией записей,
    /// имеющие фиксированные границы записей,
    /// при этом границы могут различаться в зависимости от позиции относительно разреза.
    /// </summary>
    public interface IDecorationColumnWithRecords<out TDecorationColumnRecord> : IDecorationColumn 
        where TDecorationColumnRecord: IDecorationColumnRecord
    {
        /// <summary>
        /// Записи в колонке.
        /// </summary>
        IReadOnlyCollection<TDecorationColumnRecord> Records { get; }
    }
}