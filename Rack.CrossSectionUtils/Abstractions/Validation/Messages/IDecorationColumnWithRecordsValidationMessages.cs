namespace Rack.CrossSectionUtils.Abstractions.Validation.Messages
{
    public interface IDecorationColumnWithRecordsValidationMessages: IDecorationColumnValidationMessages
    {
        /// <summary>
        /// Сообщение о том, что существуют несогласованность
        /// в левых границах элементов колонки
        /// (границы нескольких элементов колонок пересекаются).
        /// </summary>
        string ConflictInLeftBorders { get; }

        /// <summary>
        /// Сообщение о том, что существуют несогласованность
        /// в левых границах элементов колонки
        /// (границы нескольких элементов колонок пересекаются).
        /// </summary>
        string ConflictInRightBorders { get; }
    }
}