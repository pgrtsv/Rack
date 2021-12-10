namespace Rack.Localization
{
    public static class StringExtensions
    {
        /// <summary>
        /// Эквивалентно string.Format(), но не выбрасывает исключения, если string.IsNullorEmpty(<see cref="text"/>).
        /// </summary>
        /// <param name="text">Форматируемый текст.</param>
        /// <param name="args">Аргументы.</param>
        public static string FormatDefault(this string text, params object[] args)
        {
            return string.IsNullOrEmpty(text) ? string.Empty : string.Format(text, args);
        }
    }
}