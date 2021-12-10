namespace Rack.LocalizationTool.Models.LocalizationProblem
{
    /// <summary>
    /// Нелокализованная строка в файле.
    /// </summary>
    public interface IUnlocalizedString
    {
        /// <summary>
        /// Обнаруженная нелокализованная строка.
        /// </summary>
        string String { get; }

        /// <summary>
        /// Запись в обнаруженной строке (т. е. <see cref="String"/> без кавычек, знаков экранирования/интерполяции и пр.).
        /// </summary>
        string Value { get; }

        /// <summary>
        /// Номер строки, в которой располагается <see cref="String"/>.
        /// </summary>
        int Row { get; }

        /// <summary>
        /// Индекс, по которому располагается <see cref="String"/>.
        /// </summary>
        int Index { get; }
    }
}