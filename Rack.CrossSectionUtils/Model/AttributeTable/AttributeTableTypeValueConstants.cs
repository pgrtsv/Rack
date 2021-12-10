namespace Rack.CrossSectionUtils.Model.AttributeTable
{
    /// <summary>
    /// Константы, принятые по соглашению,
    /// для типовых значений атрибута <see cref="AttributeTableNamesConstants.Type"/>,
    /// представляющий тип полигона (элемента колонки оформления).
    /// </summary>
    public class AttributeTableTypeValueConstants
    {
        /// <summary>
        /// Заголовок.
        /// </summary>
        public const string Header = nameof(Header);

        /// <summary>
        /// Задний фон.
        /// </summary>
        public const string Background = nameof(Background);

        /// <summary>
        /// Числовая отметка (на линейке).
        /// </summary>
        public const string Scale = nameof(Scale);

        /// <summary>
        /// Ячейка с произвольным содержанием (как правило текст).
        /// </summary>
        public const string Content = nameof(Content);
    }
}