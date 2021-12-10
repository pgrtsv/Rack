using System.Collections.Generic;
using Rack.CrossSectionUtils.Abstractions.Model;

namespace Rack.CrossSectionUtils.Model
{
    /// <inheritdoc cref="IDecorationColumnWithRecords{TDecorationColumnRecord}"/>
    public class DecorationColumnWithRecords: IDecorationColumnWithRecords<DecorationColumnRecord>
    {
        /// <inheritdoc />
        public DecorationColumnWithRecords(
            string header, 
            DecorationColumnMode mode, 
            IReadOnlyCollection<DecorationColumnRecord> records)
        {
            Header = header;
            Mode = mode;
            Records = records;
        }

        /// <inheritdoc />
        public string Header { get; }

        /// <inheritdoc />
        public DecorationColumnMode Mode { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<DecorationColumnRecord> Records { get; }
    }
}