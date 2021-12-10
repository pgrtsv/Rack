using Rack.CrossSectionUtils.Abstractions.Validation.Messages;

namespace Rack.CrossSectionUtils.Validation.Messages
{
    public class DecorationColumnWithRecordsValidationMessages: 
        DecorationColumnValidationMessages, IDecorationColumnWithRecordsValidationMessages
    {
        /// <inheritdoc />
        public string ConflictInLeftBorders =>
            "Существуют несогласованность в левых границах у элементов колонки " +
            "(границы нескольких элементов пересекаются).";

        /// <inheritdoc />
        public string ConflictInRightBorders =>
            "Существуют несогласованность в правых границах у элементов колонки " +
            "(границы нескольких элементов пересекаются).";
    }
}