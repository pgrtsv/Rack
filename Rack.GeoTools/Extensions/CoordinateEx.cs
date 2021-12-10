using NetTopologySuite.Geometries;

namespace Rack.GeoTools.Extensions
{
    public static class CoordinateEx
    {
        /// <summary>
        /// Возвращает точку, расположенную на прямой, которую образуют две координаты.
        /// Точка отстоит от первой координаты на заданном расстоянии отступа.
        /// </summary>
        /// <param name="firstCoordinate">Первая координата.</param>
        /// <param name="secondCoordinate">Вторая координата.</param>
        /// <param name="offset">Расстояние отступа от первой координаты.</param>
        /// <param name="findInnerPoint">
        /// Если <see cref="false"/>, возвращаемая точка располагается на отрезке,
        /// который образуют координаты.
        /// Если <see cref="true"/>, возвращаемая точка располагается вне отрезка.
        /// </param>
        public static Coordinate GetOffsetCoordinate(
            this Coordinate firstCoordinate,
            Coordinate secondCoordinate,
            double offset,
            bool findInnerPoint = false)
        {
            var line = new LineSegment(firstCoordinate, secondCoordinate);
            var offsetFraction = offset / line.Length;
            if (findInnerPoint && offsetFraction > 1)
                offsetFraction = 1;
            return findInnerPoint
                ? line.PointAlong(offsetFraction)
                : line.PointAlong(-offsetFraction);
        }

        /// <summary>
        /// Возвращает прямоугольный полигон,
        /// образованный координатами двух противоположных вершин (по диагонали).
        /// </summary>
        /// <param name="startCoordinate">Координата первой вершины.</param>
        /// <param name="endCoordinate">Координата второй вершины.</param>
        public static Polygon CreateRectangle(
            this Coordinate startCoordinate, 
            Coordinate endCoordinate) =>
            new Polygon(new LinearRing(new[]
            {
                startCoordinate,
                new Coordinate(startCoordinate.X, endCoordinate.Y),
                endCoordinate,
                new Coordinate(endCoordinate.X, startCoordinate.Y),
                startCoordinate
            }));
    }
}