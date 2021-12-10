using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using Rack.CrossSectionUtils.Abstractions.Model;
using Rack.CrossSectionUtils.Extensions;
using Rack.GeoTools.Abstractions.Model;
using Rack.GeoTools.Extensions;
using UnitsNet;

namespace Rack.CrossSectionUtils.Utils
{
    /// <summary>
    /// Методы для расчёта данных разреза.
    /// </summary>
    public static class CrossSectionDataCalculationMethods
    {
        /// <summary>
        /// Возвращает линии, представляющие горизонтальные поверхности
        /// на изображении разреза.
        /// Детальность линий рассчитывается из горизонтального разрешения
        /// <see cref="ICrossSectionBuildSettings.HorizontalResolution"/>.
        /// </summary>
        /// <typeparam name="TSurface">Тип горизонтальной поверхности.</typeparam>
        /// <param name="surfaces">Горизонтальные поверхности
        /// (единицы измерения координат - метры).</param>
        /// <param name="crossSectionLine">Линия разреза
        /// (единицы измерения координат - метры).</param>
        /// <param name="crossSectionBuildSettings">Параметры построения разреза.</param>
        /// <param name="leftDecorationColumnsLength">Общая ширина колонок оформления
        /// (слева от разреза) в системе координат отрисовки разреза.</param>
        /// <returns>Линии горизонтальных поверхностей в разрезе, в системе координат отрисовки разреза.
        /// Порядок линий в перечисление, соответствует
        /// порядку следования входных поверхностей.</returns>
        public static IEnumerable<LineString> ToCrossSectionRepresentation<TSurface>(
            this IEnumerable<TSurface> surfaces,
            LineString crossSectionLine,
            ICrossSectionBuildSettings crossSectionBuildSettings,
            Length leftDecorationColumnsLength)
            where TSurface : ISurface
        {
            var step = crossSectionBuildSettings.GetHorizontalStep().Meters;
            var coordinates = crossSectionLine.GetSampledLineString(step)
                .ToArray();

            foreach (var surface in surfaces)
            {
                var l = 0.0; // Длинна пройденого пути (м).
                /* Координаты снятые с поверхности по ломанной линии. */
                var horizonCoordinates = new List<Coordinate>(coordinates.Length);
                for (int i = 0; i < coordinates.Length; i++)
                {
                    var currentCoordinate = coordinates[i];
                    l += i == 0
                        ? 0
                        : coordinates[i - 1].Distance(currentCoordinate);

                    if (!surface.Envelope.Contains(currentCoordinate))
                        continue;
                    var h = surface.GetZ(currentCoordinate.X, currentCoordinate.Y);

                    var x = l.TransformToCrossSectionXAxis(
                        crossSectionBuildSettings, leftDecorationColumnsLength);
                    var y = h.TransformToCrossSectionYAxis(
                        crossSectionBuildSettings);
                    horizonCoordinates.Add(new Coordinate(x, y));
                }

                yield return new LineString(horizonCoordinates.ToArray());
            }
        }
    }
}