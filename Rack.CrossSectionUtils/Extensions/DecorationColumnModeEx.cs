using Rack.CrossSectionUtils.Model;

namespace Rack.CrossSectionUtils.Extensions
{
    /// <summary>
    /// Методы расширения для режима отрисовки колонки оформления <see cref="DecorationColumnMode"/>.
    /// </summary>
    public static class DecorationColumnModeEx
    {
        /// <summary>
        /// Проверяет, режим отрисвки предпологает отрисвоку по левому краю.
        /// </summary>
        /// <param name="mode">Режим отрисвки.</param>
        /// <returns><see cref="true"/>, если режим отрисвки предпологает отрисвоку по левому краю.</returns>
        public static bool HasLeft(this DecorationColumnMode mode) =>
            mode == DecorationColumnMode.Left || mode == DecorationColumnMode.LeftAndRight;

        /// <summary>
        /// Проверяет, режим отрисвки предпологает отрисвоку по правому краю.
        /// </summary>
        /// <param name="mode">Режим отрисвки.</param>
        /// <returns><see cref="true"/>, если режим отрисвки предпологает отрисвоку по правому краю.</returns>
        public static bool HasRight(this DecorationColumnMode mode) =>
            mode == DecorationColumnMode.Right || mode == DecorationColumnMode.LeftAndRight;

        /// <summary>
        /// Включает отрисовку по левому краю в режим отрисовки.
        /// </summary>
        /// <param name="mode">Режим отрисовки.</param>
        public static DecorationColumnMode AddLeft(this DecorationColumnMode mode) => mode switch
        {
            DecorationColumnMode.None => DecorationColumnMode.Left,
            DecorationColumnMode.Left => DecorationColumnMode.Left,
            DecorationColumnMode.Right => DecorationColumnMode.LeftAndRight,
            DecorationColumnMode.LeftAndRight => DecorationColumnMode.LeftAndRight
        };

        /// <summary>
        /// Включает отрисовку по правому краю в режим отрисовки.
        /// </summary>
        /// <param name="mode">Режим отрисовки.</param>
        public static DecorationColumnMode AddRight(this DecorationColumnMode mode) => mode switch
        {
            DecorationColumnMode.None => DecorationColumnMode.Right,
            DecorationColumnMode.Left => DecorationColumnMode.LeftAndRight,
            DecorationColumnMode.Right => DecorationColumnMode.Right,
            DecorationColumnMode.LeftAndRight => DecorationColumnMode.LeftAndRight
        };
    }
}