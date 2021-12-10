using Rack.CrossSectionUtils.Abstractions.Model;
using UnitsNet;
using UnitsNet.Units;

namespace Rack.CrossSectionUtils.Extensions
{
    /// <summary>
    /// Методы расширения для трансформаций значения между
    /// фактической системой координат и системой координат для отрисовки разреза.
    /// </summary>
    public static class TransformLengthValueEx
    {
        /// <summary>
        /// Трансформирует координату по оси X из фактической системы координат
        /// в систему координат для отрисовки разреза (в см).
        /// </summary>
        /// <param name="value">Значение в фактической системе координат.</param>
        /// <param name="settings">Параметры построения разреза.</param>
        /// <param name="xOffset">Смещение по оси X в системе координат для отрисовки разреза,
        /// равное общей длине всех колонок оформления.</param>
        /// <param name="lengthUnit">Еденицы измерения <see cref="value"/>,
        /// по умолчанию <see cref="LengthUnit.Meter"/>.</param>
        public static double TransformToCrossSectionXAxis(
            this double value,
            ICrossSectionBuildSettings settings,
            Length xOffset,
            LengthUnit lengthUnit = LengthUnit.Meter)
        {
            return new Length(value, lengthUnit)
                .TransformToCrossSectionXAxis(settings, xOffset)
                .Centimeters;
        }

        /// <summary>
        /// Трансформирует координату по оси X из фактической системы координат
        /// в систему координат для отрисовки разреза.
        /// </summary>
        /// <param name="value">Значение в фактической системе координат.</param>
        /// <param name="settings">Параметры построения разреза.</param>
        /// <param name="xOffset">Смещение по оси X в системе координат для отрисовки разреза,
        /// равное общей длине всех колонок оформления.</param>
        public static Length TransformToCrossSectionXAxis(
            this Length value,
            ICrossSectionBuildSettings settings,
            Length xOffset) =>
            value * settings.HorizontalScale + xOffset;

        /// <summary>
        /// Трансформирует координату (в см) по оси X
        /// из системы координат отрисовки разреза к фактической системе координат.
        /// </summary>
        /// <param name="value">Значение в системе координат отрисовки разреза.</param>
        /// <param name="settings">Параметры построения разреза.</param>
        /// <param name="xOffset">Смещение по оси X в системе координат для отрисовки разреза,
        /// равное общей длине всех колонок оформления.</param>
        public static Length TransformToRealXAxis(
            this double value,
            ICrossSectionBuildSettings settings,
            Length xOffset)
        {
            return (new Length(value, LengthUnit.Centimeter) - xOffset) / settings.HorizontalScale;
        }

        /// <summary>
        /// Трансформирует координату по оси Y из фактической системы координат
        /// в систему координат для отрисовки разреза (в см).
        /// </summary>
        /// <param name="value">Значение в фактической системе координат.</param>
        /// <param name="settings">Параметры построения разреза.</param>
        /// <param name="lengthUnit">Еденицы измерения <see cref="value"/>,
        /// по умолчанию <see cref="LengthUnit.Meter"/>.</param>
        public static double TransformToCrossSectionYAxis(
            this double value,
            ICrossSectionBuildSettings settings,
            LengthUnit lengthUnit = LengthUnit.Meter) =>
            new Length(value, lengthUnit)
                .TransformToCrossSectionYAxis(settings)
                .Centimeters;

        /// <summary>
        /// Трансформирует координату по оси Y из фактической системы координат
        /// в систему координат для отрисовки разреза.
        /// </summary>
        /// <param name="value">Значение в фактической системе координат.</param>
        /// <param name="settings">Параметры построения разреза.</param>
        public static Length TransformToCrossSectionYAxis(
            this Length value,
            ICrossSectionBuildSettings settings) =>
            (value - settings.Bottom) * settings.VerticalScale;

        /// <summary>
        /// Трансформирует координату (в см) по оси Y
        /// из системы координат отрисовки разреза к фактической системе координат.
        /// </summary>
        /// <param name="value">Значение в системе координат отрисовки разреза.</param>
        /// <param name="settings">Параметры построения разреза.</param>
        public static Length TransformToRealYAxis(
            this double value,
            ICrossSectionBuildSettings settings)
        {
            return new Length(
                value / settings.VerticalScale + settings.Bottom.Centimeters,
                LengthUnit.Centimeter);
        }
    }
}