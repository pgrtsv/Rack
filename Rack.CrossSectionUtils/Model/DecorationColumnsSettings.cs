using System.Collections.Generic;
using System.Linq;
using Rack.CrossSectionUtils.Abstractions.Model;

namespace Rack.CrossSectionUtils.Model
{
    /// <inheritdoc cref="IDecorationColumnsSettings{TRuler,TDecorationColumn,TRecord}"/>
    public class DecorationColumnsSettings<TRuler, TDecorationColumn, TRecord>:
        IDecorationColumnsSettings<TRuler, TDecorationColumn, TRecord>
        where TRuler : IDecorationColumn
        where TDecorationColumn : IDecorationColumnWithRecords<TRecord>
        where TRecord : IDecorationColumnRecord
    {
        public DecorationColumnsSettings(
            TRuler ruler,
            IEnumerable<TDecorationColumn> decorationColumnsWithRecords)
        {
            DecorationColumnsWithRecords = decorationColumnsWithRecords.ToArray();
            Ruler = ruler;
        }

        /// <inheritdoc/>
        public TRuler Ruler { get; }

        /// <inheritdoc/>
        public IReadOnlyCollection<TDecorationColumn> DecorationColumnsWithRecords { get; }
    }
}