using System.Collections.Generic;

namespace Rack.CrossSectionUtils.Abstractions.Model
{
    /// <summary>
    /// Настройки для колонок оформления разреза и линейки высот.
    /// </summary>
    /// <typeparam name="TRuler">Тип линейки.</typeparam>
    /// <typeparam name="TDecorationColumn">Тип колонки оформления
    /// с фиксированными границами ячеек.</typeparam>
    /// <typeparam name="TRecord">Тип ячейки для <see cref="TDecorationColumn"/>.</typeparam>
    public interface IDecorationColumnsSettings<out TRuler, out TDecorationColumn, TRecord>
        where TRuler : IDecorationColumn
        where TDecorationColumn : IDecorationColumnWithRecords<TRecord>
        where TRecord : IDecorationColumnRecord
    {
        /// <summary>
        /// Линейка асболютных отметок.
        /// </summary>
        TRuler Ruler { get; }

        /// <summary>
        /// Колонки оформления (с фиксированными границами) в порядке их отрисовки.
        /// </summary>
        IReadOnlyCollection<TDecorationColumn> DecorationColumnsWithRecords { get; }
    }
}