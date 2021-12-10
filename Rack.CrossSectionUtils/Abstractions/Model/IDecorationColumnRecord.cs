using UnitsNet;

namespace Rack.CrossSectionUtils.Abstractions.Model
{
    /// <summary>
    /// Запись в колонке для оформления разреза.
    /// </summary>
    public interface IDecorationColumnRecord
    {
        /// <summary>
        /// Текст записи.
        /// </summary>
        string Text { get; }

        /// <summary>
        /// Нижняя граница для отрисовки слева от разреза.
        /// </summary>
        Length LeftBottom { get; }

        /// <summary>
        /// Верхняя граница для отрисовки слева от разреза.
        /// </summary>
        Length LeftTop { get; }

        /// <summary>
        /// Нижняя граница для отрисовки справа от разреза.
        /// </summary>
        Length RightBottom { get; }

        /// <summary>
        /// Верхняя граница для отрисовки справа от разреза.
        /// </summary>
        Length RightTop { get; }
    }
}