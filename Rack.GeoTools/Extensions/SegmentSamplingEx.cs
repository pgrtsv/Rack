using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NetTopologySuite.Geometries;

namespace Rack.GeoTools.Extensions
{
    /// <summary>
    /// Класс методов расширения для дискретизации отрезка, т. е.
    /// преобразования отрезка в группу координат.
    /// </summary>
    public static class SegmentSamplingEx
    {
        /// <summary>
        /// Выполняет дискретизацию ломаной линии.
        /// Итоговый результат формируется за счёт независимой дискретезации каждого отрезка:
        /// вторая точка в группе дискретизированных точек отрезка
        /// всегда удалена от первой точки на расстояние, равное (в случае последней точки отрезка - меньшее) шагу.
        /// При этом все точки исходной ломаной линии обязательно включаются в выходной результат.
        /// </summary>
        /// <param name="lineString">Ломаная линия.</param>
        /// <param name="step">Шаг.</param>
        /// <returns>Перечисление координат.</returns>
        public static IEnumerable<Coordinate> GetSampledLineString(
            this LineString lineString,
            double step)
        {
            return lineString.Coordinates.SkipLast(1)
                .Zip(lineString.Coordinates.Skip(1))
                .SelectMany(segment =>
                {
                    var (startSegment, endSegment) = segment;
                    var segmentCoordinates = GetSampledSegment(
                            startSegment, endSegment,
                            step)
                        .ToArray();
                    return endSegment != lineString.Coordinates.Last()
                        ? segmentCoordinates.SkipLast(1)
                        : segmentCoordinates;
                });
        }

        /// <summary>
        /// Выполняет дискретизацию отрезка: преобразует отрезок в группу координат,
        /// удаленных друг от друга на заданный шаг.
        /// </summary>
        /// <param name="startCoordinate">Координата начала отрезка.</param>
        /// <param name="endCoordinate">Координата конца отрезка.</param>
        /// <param name="step">Шаг.</param>
        /// <param name="isEndPointIncluded"><see langword="true"/>,
        /// если в выходное перечисление координат обязательно включается конец отрезка.</param>
        /// <param name="precision">Точность сравнения координат.
        /// Актуальна, если <see cref="isEndPointIncluded"/>=<see langword="true"/>.</param>
        /// <returns>Перечисление координат.</returns>
        public static IEnumerable<Coordinate> GetSampledSegment(
            this Coordinate startCoordinate,
            Coordinate endCoordinate,
            double step,
            bool isEndPointIncluded = true,
            int precision = 3) =>
            new LineSegment(startCoordinate, endCoordinate)
                .GetSampledSegment(step, isEndPointIncluded, precision);

        /// <summary>
        /// Выполняет дискретизацию отрезка: преобразует отрезок в группу координат,
        /// удаленных друг от друга на заданный шаг.
        /// </summary>
        /// <param name="lineSegment">Отрезок.</param>
        /// <param name="step">Шаг.</param>
        /// <param name="isEndPointIncluded"><see langword="true"/>,
        /// если в выходное перечисление координат обязательно включается конец отрезка.</param>
        /// <param name="precision">Точность сравнения координат.
        /// Актуальна, если <see cref="isEndPointIncluded"/>=<see langword="true"/>.</param>
        /// <returns>Перечисление координат.</returns>
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public static IEnumerable<Coordinate> GetSampledSegment(
            this LineSegment lineSegment,
            double step,
            bool isEndPointIncluded = true,
            int precision = 3)
        {
            var currentCoordinate = lineSegment.P0;
            yield return currentCoordinate;

            while (currentCoordinate.Distance(lineSegment.P1) >= step)
            {
                currentCoordinate = currentCoordinate
                    .GetOffsetCoordinate(lineSegment.P1, step, true);
                yield return currentCoordinate;
            }

            if (isEndPointIncluded &&
                (Math.Round(currentCoordinate.X, precision) != Math.Round(lineSegment.P1.X, precision) ||
                 Math.Round(currentCoordinate.Y, precision) != Math.Round(lineSegment.P1.Y, precision)))
                yield return lineSegment.P1;
        }
    }
}