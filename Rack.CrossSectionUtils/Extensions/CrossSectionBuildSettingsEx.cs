using System.Linq;
using Rack.CrossSectionUtils.Abstractions.Model;
using UnitsNet;
using UnitsNet.Units;

namespace Rack.CrossSectionUtils.Extensions
{
    /// <summary>
    /// Методы расширения для параметров построения разреза.
    /// </summary>
    public static class CrossSectionBuildSettingsEx
    {
        /// <summary>
        /// Возвращает высоту разреза.
        /// </summary>
        /// <param name="settings">Параметры построения разреза.</param>
        public static Length GetHeight(this ICrossSectionBuildSettings settings) => 
            (settings.Top - settings.Bottom) * settings.VerticalScale;

        /// <summary>
        /// Возвращает сколько метров содержится в 1 см разреза по оси Y.
        /// </summary>
        /// <param name="settings">Параметры построения разреза.</param>
        public static int GetMetersToCentimeterAtVertical(this ICrossSectionBuildSettings settings) =>
            (int) (1.0 / settings.VerticalScale / 100);

        /// <summary>
        /// Рассчитывает шаг по горизонтали.
        /// </summary>
        /// <param name="settings">Параметры построения разреза.</param>
        public static Length GetHorizontalStep(this ICrossSectionBuildSettings settings)
        {
            var step = new Length(1.0 / settings.HorizontalResolution, LengthUnit.Centimeter);
            return step / settings.HorizontalScale;
        }

        /// <summary>
        /// Рассчитывает шаг по вертикали.
        /// </summary>
        /// <param name="settings">Параметры построения разреза.</param>
        public static Length GetVerticalStep(this ICrossSectionBuildSettings settings)
        {
            var step = new Length(1.0 / settings.VerticalResolution, LengthUnit.Centimeter);
            return step / settings.VerticalScale;
        }

        /// <summary>
        /// Рассчитывает общую длину колонок отрисовки слева.
        /// </summary>
        /// <typeparam name="TRuler">Тип линейки.</typeparam>
        /// <typeparam name="TDecorationColumn">Тип колонки оформления
        /// с фиксированными границами ячеек.</typeparam>
        /// <typeparam name="TRecord">Тип ячейки для <see cref="TDecorationColumn"/>.</typeparam>
        /// <param name="crossSectionBuildSettings">Параметры построения разреза.</param>
        /// <param name="decorationColumnsSettings">Параметры колонок оформления.</param>
        public static Length CalculateLeftDecorationColumnsLength<TRuler, TDecorationColumn, TRecord>(
            this ICrossSectionBuildSettings crossSectionBuildSettings,
            IDecorationColumnsSettings<TRuler, TDecorationColumn, TRecord> decorationColumnsSettings)
            where TRuler : IDecorationColumn
            where TDecorationColumn : IDecorationColumnWithRecords<TRecord>
            where TRecord : IDecorationColumnRecord
        {
            var decorationColumnCount = decorationColumnsSettings
                .DecorationColumnsWithRecords
                .Count(x => x.Mode.HasLeft());
            if (decorationColumnsSettings.Ruler.Mode.HasLeft())
                decorationColumnCount++;
            return crossSectionBuildSettings.DecorationColumnsWidth * decorationColumnCount;
        }
    }
}